using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdentityApi.Controllers.Resources;
using IdentityApi.Controllers.Resources.ApiError;
using IdentityApi.Core.Contracts;
using IdentityApi.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RolesController : Controller
  {
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public RolesController(RoleManager<ApplicationRole> roleManager,
                            UserManager<ApplicationUser> userManager,
                            IUnitOfWork unitOfWork,
                            IMapper mapper)
    {
      this.userManager = userManager;
      this.roleManager = roleManager;
      this.unitOfWork = unitOfWork;
      this.mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleResource roleResource)
    {
      if (ModelState.IsValid)
      {
        var applicationRole = this.mapper.Map<CreateRoleResource, ApplicationRole>(roleResource);

        var result = await roleManager.CreateAsync(applicationRole);
        if (result.Succeeded)
        {
          await this.unitOfWork.CompleteAsync();

          return new OkObjectResult(new { message = $"New role with name ({roleResource.RoleName}) was created" });
        }
        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
      var roles = this.roleManager.Roles;

      var respone = this.mapper.Map<IEnumerable<ApplicationRole>, IEnumerable<RoleResource>>(roles);

      return new OkObjectResult(respone);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRole(CreateRoleResource createRoleResource)
    {
      if (ModelState.IsValid)
      {
        var role = await roleManager.FindByIdAsync(createRoleResource.Id);

        if (role == null)
        {
          ModelState.AddModelError("", $"Role with Id: ({createRoleResource.Id}) cannot be found");
          return new NotFoundObjectResult(new NotFoundResource(ModelState));
        }
        else
        {
          this.mapper.Map<CreateRoleResource, IdentityRole>(createRoleResource, role);
          var result = await roleManager.UpdateAsync(role);

          if (result.Succeeded)
          {
            await this.unitOfWork.CompleteAsync();

            return new OkObjectResult(new { message = $"Role with Id: ({createRoleResource.Id}) was updated" });
          }

          foreach (IdentityError error in result.Errors)
          {
            ModelState.AddModelError("", error.Description);
          }
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole([FromRoute] string roleId)
    {
      var role = await roleManager.FindByIdAsync(roleId);

      if (role == null)
      {
        ModelState.AddModelError("", $"Role with Id: ({roleId}) cannot be found");
        return new NotFoundObjectResult(new NotFoundResource(ModelState));
      }

      try
      {
        var result = await roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
          await this.unitOfWork.CompleteAsync();

          return new OkObjectResult(new { message = $"Role ({role.Name}) deleted" });
        }

        foreach (var error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }

        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }
      catch (DbUpdateException e)
      {
        ModelState.AddModelError("", $"({role.Name}) is in use");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }
    }
  }
}