using System.Threading.Tasks;

namespace IdentityApi.Core.Contracts
{
  public interface IUnitOfWork
  {
    Task CompleteAsync();
  }
}