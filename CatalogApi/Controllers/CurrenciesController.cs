using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class CurrenciesController : Controller
  {
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "USD", "EUR", "SP" };
    }
  }
}
