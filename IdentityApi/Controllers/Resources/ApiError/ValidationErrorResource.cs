using Newtonsoft.Json;

namespace IdentityApi.Controllers.Resources.ApiError
{
  public class ValidationErrorResource
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Field { get; }

    public string Message { get; }

    public ValidationErrorResource(string field, string message)
    {
      Field = field != string.Empty ? field : null;
      Message = message;
    }
  }
}