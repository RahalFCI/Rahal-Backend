using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence
{
    public class UsersDBInitializer(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<UsersDBInitializer> logger) : IDbInitializer
    {
        public async Task SeedAsync()
        {
            foreach (UserRoleEnum userTypeEnum in Enum.GetValues(typeof(UserRoleEnum)))
            {
                var roleName = userTypeEnum.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }

            await SeedAdminUserAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            var email = configuration["AdminUser:Email"];
            var password = configuration["AdminUser:Password"];
            var displayName = configuration["AdminUser:DisplayName"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation("Admin user seeding skipped because AdminUser:Email or AdminUser:Password is not configured.");
                return;
            }

            var adminRole = UserRoleEnum.Admin.ToString();
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is null)
            {
                var adminUser = new User
                {
                    UserName = email,
                    Email = email,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
                    UserType = UserRoleEnum.Admin,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, password);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed to seed admin user {Email}. Errors: {Errors}",
                        email,
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return;
                }

                var roleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to assign admin role to seeded user {Email}. Errors: {Errors}",
                        email,
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return;
                }

                logger.LogInformation("Seeded admin user {Email}.", email);
                return;
            }

            var changed = false;

            if (existingUser.UserType != UserRoleEnum.Admin)
            {
                existingUser.UserType = UserRoleEnum.Admin;
                changed = true;
            }

            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(displayName) && existingUser.DisplayName != displayName)
            {
                existingUser.DisplayName = displayName;
                changed = true;
            }

            if (changed)
            {
                var updateResult = await userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    logger.LogError("Failed to update existing admin user {Email}. Errors: {Errors}",
                        email,
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return;
                }
            }

            if (!await userManager.CheckPasswordAsync(existingUser, password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingUser);
                var passwordResult = await userManager.ResetPasswordAsync(existingUser, resetToken, password);
                if (!passwordResult.Succeeded)
                {
                    logger.LogError("Failed to update seeded admin password for {Email}. Errors: {Errors}",
                        email,
                        string.Join(", ", passwordResult.Errors.Select(e => e.Description)));
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(existingUser, adminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(existingUser, adminRole);
                if (!roleResult.Succeeded)
                {
                    logger.LogError("Failed to assign admin role to existing user {Email}. Errors: {Errors}",
                        email,
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
