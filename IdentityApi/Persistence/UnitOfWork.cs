using System.Threading.Tasks;
using IdentityApi.Core.Contracts;

namespace IdentityApi.Persistence
{
  public class UnitOfWork : IUnitOfWork
  {
    private readonly ApplicationDbContext context;
    public UnitOfWork(ApplicationDbContext context)
    {
      this.context = context;

    }

    public async Task CompleteAsync()
    {
      await this.context.SaveChangesAsync();
    }
  }
}