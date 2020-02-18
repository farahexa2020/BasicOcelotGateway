using System.Threading.Tasks;
using IdentityApi.Core.Models;

namespace IdentityApi.Core.Contracts
{
  public interface IRefreshTokenRepository
  {
    Task<RefreshToken> GetRefreshTokenAsync(RefreshToken refreshToken);
    void Add(RefreshToken refreshToken);
    void Remove(RefreshToken refreshToken);
  }
}