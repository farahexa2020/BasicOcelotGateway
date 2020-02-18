using System.Net;
using IdentityApi.Controllers.Resources.ApiError;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ErrorsController : Controller
  {
    [Route("{code}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error(int code)
    {
      HttpStatusCode parsedCode = (HttpStatusCode)code;

      var error = new ApiErrorResource(code, parsedCode.ToString());

      return new ObjectResult(error);
    }
  }
}