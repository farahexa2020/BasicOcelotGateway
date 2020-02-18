using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers.Resources
{
  public class ResetPasswordResource
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 8)]
    public string Password { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 8)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
  }
}