using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class BadRequestResource : ApiErrorResource
  {
    public BadRequestResource()
: base(400, HttpStatusCode.BadRequest.ToString())
    {
    }

    public BadRequestResource(ModelStateDictionary modelState)
        : base(400, HttpStatusCode.BadRequest.ToString(), modelState)
    {
    }
  }
}