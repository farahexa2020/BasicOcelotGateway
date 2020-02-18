using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityApi.Core.Models
{
  public class ApplicationUser : IdentityUser
  {
    [Required]
    public string FirstName { get; set; }

    [Required]
    [EmailAddress]
    public string LastName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    public ApplicationUser()
    {
      this.UserRoles = new List<ApplicationUserRole>();
    }
  }
}