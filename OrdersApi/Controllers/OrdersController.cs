using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace OrdersApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class OrdersController : Controller
  {
    [HttpGet]
    public IEnumerable<Order> Get()
    {
      var order1 = new Order("1001", "1001", 100);
      var order2 = new Order("1002", "1001", 200);
      var order3 = new Order("1002", "1002", 200);
      return new List<Order> { order1, order2, order3 };
    }

    public class Order
    {
      public string Id { get; set; }
      public string ProductId { get; set; }
      public int Amount { get; set; }

      public Order(string id, string productId, int amount)
      {
        this.Id = id;
        this.ProductId = productId;
        this.Amount = amount;
      }
    }
  }
}
