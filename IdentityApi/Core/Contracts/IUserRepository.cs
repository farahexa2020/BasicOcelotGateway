using System.Threading.Tasks;
using IdentityApi.Core.Models;
using IdentityApi.QueryModels;

namespace IdentityApi.Core.Contracts
{
  public interface IUserRepository
  {
    Task<QueryResult<ApplicationUser>> GetUsersAsync(UserQuery queryObj);
    Task<ApplicationUser> FindUserByIdAsync(string id);
  }
}