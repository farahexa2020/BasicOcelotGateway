using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class NotFoundResource : ApiErrorResource
  {
    public NotFoundResource()
        : base(404, HttpStatusCode.NotFound.ToString())
    {
    }

    public NotFoundResource(ModelStateDictionary modelState)
        : base(404, HttpStatusCode.NotFound.ToString(), modelState)
    {
    }
  }
}