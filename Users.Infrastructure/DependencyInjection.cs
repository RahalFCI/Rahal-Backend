using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory", "users")
                )
            );


            //Enable Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;

                //Password Complexity Configuration
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
                .AddEntityFrameworkStores<UsersDbContext>()
                .AddDefaultTokenProviders() //Generate Token for reset password, change email, etc
                .AddUserStore<UserStore<User, Role, UsersDbContext, Guid>>()
                .AddRoleStore<RoleStore<Role, UsersDbContext, Guid>>();

            

            return services;
        }
    }
}
