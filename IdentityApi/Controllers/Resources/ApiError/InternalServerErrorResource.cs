using System.Net;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class InternalServerErrorResource : ApiErrorResource
  {
    public InternalServerErrorResource()
        : base(500, HttpStatusCode.InternalServerError.ToString())
    {
    }
  }
}