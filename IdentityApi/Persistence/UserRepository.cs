using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityApi.Core.Contracts;
using IdentityApi.Core.Models;
using IdentityApi.Extensions;
using IdentityApi.QueryModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Persistence
{
  public class UserRepository : IUserRepository
  {
    public UserManager<ApplicationUser> userManager { get; set; }
    public UserRepository(UserManager<ApplicationUser> userManager)
    {
      this.userManager = userManager;

    }

    private enum QueryFilter
    {
      FirstName = 1,
      LastName = 2,
      Email = 3,
      IsActive = 4,
      RoleId = 5
    }

    public async Task<QueryResult<ApplicationUser>> GetUsersAsync(UserQuery queryObj)
    {
      var result = new QueryResult<ApplicationUser>();

      var query = userManager.Users.Include(u => u.UserRoles)
                                  .ThenInclude(ur => ur.Role)
                                  .AsQueryable();

      var filterColumnsMap = new Dictionary<string, Expression<Func<ApplicationUser, bool>>>()
      {
        ["FirstName"] = u => u.FirstName == queryObj.FirstName,
        ["LastName"] = u => u.LastName == queryObj.LastName,
        ["Email"] = u => u.Email == queryObj.Email,
        ["IsActive"] = u => u.IsActive == queryObj.IsActive,
        ["RoleId"] = u => u.UserRoles.Select(ur => ur.RoleId).Contains(queryObj.RoleId)
      };

      query = this.ApplyUserFiltering(query, queryObj, filterColumnsMap);

      var orderoColumnsMap = new Dictionary<string, Expression<Func<ApplicationUser, object>>>()
      {
        ["fisrtName"] = u => u.FirstName,
        ["lastName"] = u => u.LastName,
        ["email"] = u => u.Email,
      };

      result.TotalItems = await query.CountAsync();

      query = query.ApplyOrdering(queryObj, orderoColumnsMap);

      query = query.ApplyPaging(queryObj);

      result.Items = await query.ToListAsync();

      return result;
    }

    public async Task<ApplicationUser> FindUserByIdAsync(string id)
    {

      var user = await this.userManager.Users
                                      .Where(u => u.Id == id)
                                      .Include(u => u.UserRoles)
                                      .ThenInclude(ur => ur.Role)
                                      .SingleOrDefaultAsync();

      return user;
    }

    private IQueryable<ApplicationUser> ApplyUserFiltering(IQueryable<ApplicationUser> query, UserQuery queryObj, Dictionary<string, Expression<Func<ApplicationUser, bool>>> columnsMap)
    {
      if (!string.IsNullOrWhiteSpace(queryObj.FirstName) && columnsMap.ContainsKey(QueryFilter.FirstName.ToString()))
      {
        query = query.Where(columnsMap[QueryFilter.FirstName.ToString()]);
      }

      if (!string.IsNullOrWhiteSpace(queryObj.LastName) && columnsMap.ContainsKey(QueryFilter.LastName.ToString()))
      {
        query = query.Where(columnsMap[QueryFilter.LastName.ToString()]);
      }

      if (!string.IsNullOrWhiteSpace(queryObj.Email) && columnsMap.ContainsKey(QueryFilter.Email.ToString()))
      {
        query = query.Where(columnsMap[QueryFilter.Email.ToString()]);
      }

      if (queryObj.IsActive.HasValue && columnsMap.ContainsKey(QueryFilter.IsActive.ToString()))
      {
        query = query.Where(columnsMap[QueryFilter.IsActive.ToString()]);
      }

      if (!string.IsNullOrWhiteSpace(queryObj.RoleId) && columnsMap.ContainsKey(QueryFilter.RoleId.ToString()))
      {
        query = query.Where(columnsMap[QueryFilter.RoleId.ToString()]);
      }

      return query;
    }
  }
}