using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers.Resources
{
  public class EmailConfirmationResource
  {
    [Required]
    public string UserId { get; set; }

    [Required]
    public string Token { get; set; }
  }
}