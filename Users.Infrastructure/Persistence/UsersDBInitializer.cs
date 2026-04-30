using Microsoft.AspNetCore.Identity;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Infrastructure.Persistence
{
    public class UsersDBInitializer(RoleManager<Role> roleManager) : IDbInitializer
    {
      
        public async Task SeedAsync()
        {
            //Seed Roles
            foreach (UserRoleEnum userTypeEnum in Enum.GetValues(typeof(UserRoleEnum)))
            {
                if (!await roleManager.RoleExistsAsync(userTypeEnum.ToString()))
                    await roleManager.CreateAsync(new Role { Name = userTypeEnum.ToString() });

            }
        }

    }
}
