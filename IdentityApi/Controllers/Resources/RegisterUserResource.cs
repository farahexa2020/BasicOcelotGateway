using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers.Resources
{
  public class RegisterUserResource
  {
    [Required]
    [StringLength(255)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(255)]
    public string LastName { get; set; }

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