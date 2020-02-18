using System.Collections.Generic;

namespace IdentityApi.QueryModels
{
  public class QueryResult<T>
  {
    public int TotalItems { get; set; }

    public IEnumerable<T> Items { get; set; }
  }
}