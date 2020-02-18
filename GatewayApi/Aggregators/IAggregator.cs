using System.Collections.Generic;
using System.Threading.Tasks;
using Ocelot.Middleware;

namespace GatewayApi.Aggregators
{
  public interface IAggregator
  {
    Task<DownstreamResponse> Aggregate(List<DownstreamContext> responses);
  }
}