using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityApi.Core.Models
{
  [Table("RefreshTokens")]
  public class RefreshToken
  {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }

    [Required]
    public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    [Required]
    public string AccessToken { get; set; }

    [Required]
    [StringLength(255)]
    public string Token { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}