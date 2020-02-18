using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers.Resources
{
  public class RefreshTokenResource
  {
    [Required]
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
  }
}