using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class UnAuthorizedResource : ApiErrorResource
  {
    public UnAuthorizedResource()
          : base(401, HttpStatusCode.Unauthorized.ToString())
    {
    }

    public UnAuthorizedResource(ModelStateDictionary modelState)
        : base(401, HttpStatusCode.Unauthorized.ToString(), modelState)
    {
    }
  }
}