using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Middleware;
using Ocelot.Middleware.Multiplexer;

namespace GatewayApi.Aggregators
{
  public class Aggregator : IDefinedAggregator
  {
    public async Task<DownstreamResponse> Aggregate(List<DownstreamContext> responses)
    {
      var xResponseContent = await responses.FirstOrDefault(r => r.DownstreamReRoute.Key.Equals("orders")).DownstreamResponse.Content.ReadAsStringAsync();

      var yResponseContent = await responses.FirstOrDefault(r => r.DownstreamReRoute.Key.Equals("products")).DownstreamResponse.Content.ReadAsStringAsync();

      var contentBuilder = new StringBuilder();
      contentBuilder.Append(xResponseContent);
      contentBuilder.Append(yResponseContent);

      var stringContent = new StringContent(contentBuilder.ToString())
      {
        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
      };

      return new DownstreamResponse(stringContent, HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "OK");

    }
  }

  public class Order
  {
    public string Id { get; set; }
    public string ProductId { get; set; }
    public int Amount { get; set; }
  }
}