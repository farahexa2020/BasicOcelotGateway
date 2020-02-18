using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers.Resources
{
  public class CreateRoleResource
  {
    public string Id { get; set; }

    [Required]
    public string RoleName { get; set; }
  }
}