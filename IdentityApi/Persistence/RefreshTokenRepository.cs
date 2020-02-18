using System.Linq;
using System.Threading.Tasks;
using IdentityApi.Core.Contracts;
using IdentityApi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Persistence
{
  public class RefreshTokenRepository : IRefreshTokenRepository
  {
    private readonly ApplicationDbContext context;
    public RefreshTokenRepository(ApplicationDbContext context)
    {
      this.context = context;
    }

    public async Task<RefreshToken> GetRefreshTokenAsync(RefreshToken refreshToken)
    {
      return await this.context.RefreshTokens
          .Include(rt => rt.User)
          .Where(rt => rt.Token == refreshToken.Token)
          .Where(rt => rt.AccessToken == refreshToken.AccessToken)
          .FirstOrDefaultAsync();
    }

    public void Add(RefreshToken refreshToken)
    {
      this.context.Add(refreshToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
      this.context.Remove(refreshToken);
    }
  }
}