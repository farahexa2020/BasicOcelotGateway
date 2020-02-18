using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityApi.Constants;
using IdentityApi.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityApi.Persistence
{
  public class ApplicationSeedClass
  {
    public static void Seed(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
      SeedRoles(roleManager);
      SeedUsers(userManager, context);

      context.SaveChanges();
    }

    public static void SeedRoles(RoleManager<ApplicationRole> roleManager)
    {
      foreach (var role in Enum.GetValues(typeof(RolesEnum)))
      {
        var appRole = new ApplicationRole
        {
          Name = role.ToString()
        };

        if (!roleManager.RoleExistsAsync(appRole.Name).Result)
        {
          var roleResult = roleManager.CreateAsync(appRole).Result;
        }
      }
    }

    public static void SeedUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
      var user = userManager.FindByEmailAsync("admin@admin.com").Result;
      if (user == null)
      {
        user = new ApplicationUser
        {
          UserName = "admin@admin.com",
          Email = "admin@admin.com",
          FirstName = "admin",
          LastName = "admin",
          EmailConfirmed = true,
          PhoneNumber = "000-000-0000",
          PhoneNumberConfirmed = true,
          IsActive = true
        };

        var result = userManager.CreateAsync(user, "123a123a").Result;
        if (result.Succeeded)
        {
          var roleResult = userManager.AddToRolesAsync(user, new List<string> {
                              RolesEnum.Admin.ToString(),
                              RolesEnum.User.ToString()
                            }).Result;

          if (roleResult.Succeeded)
          {
            var claimResult = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, ClaimValuesEnum.SuperAdmin.ToString())).Result;
          }
        }
      }
    }
  }
}