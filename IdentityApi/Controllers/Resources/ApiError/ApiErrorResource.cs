using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class ApiErrorResource
  {
    public int StatusCode { get; private set; }

    public string StatusDescription { get; private set; }

    public IEnumerable<ValidationErrorResource> Errors { get; set; }

    public ApiErrorResource(int statusCode, string statusDescription)
    {
      this.StatusCode = statusCode;
      this.StatusDescription = statusDescription;
    }

    public ApiErrorResource(int statusCode, string statusDescription, ModelStateDictionary modelState)
        : this(statusCode, statusDescription)
    {
      this.Errors = modelState.Keys
                              .SelectMany(key => modelState[key].Errors.Select
                                (x => new ValidationErrorResource(key, x.ErrorMessage)))
                                .ToList();
    }
  }
}