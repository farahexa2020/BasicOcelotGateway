using System;
using System.Threading.Tasks;
using AutoMapper;
using IdentityApi.Constants;
using IdentityApi.Controllers.Resources;
using IdentityApi.Controllers.Resources.ApiError;
using IdentityApi.Core.Contracts;
using IdentityApi.Core.Models;
using IdentityApi.QueryModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : Controller
  {
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public UsersController(UserManager<ApplicationUser> userManager,
                            RoleManager<ApplicationRole> roleManager,
                            IUserRepository userRepository,
                            IUnitOfWork unitOfWork,
                            IMapper mapper)
    {
      this.userManager = userManager;
      this.roleManager = roleManager;
      this.userRepository = userRepository;
      this.unitOfWork = unitOfWork;
      this.mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersAsync([FromQuery] UserQueryResource userQueryResource)
    {
      var userQuery = mapper.Map<UserQueryResource, UserQuery>(userQueryResource);

      var queryResult = await this.userRepository.GetUsersAsync(userQuery);

      var response = mapper.Map<QueryResult<ApplicationUser>, QueryResultResource<UserResource>>(queryResult);

      return new OkObjectResult(response);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserByIdAsync([FromRoute] string userId)
    {
      var user = await this.userRepository.FindUserByIdAsync(userId);

      if (user == null)
      {
        ModelState.AddModelError("", "User not found");
        return new NotFoundObjectResult(new NotFoundResource(ModelState));
      }

      var response = this.mapper.Map<ApplicationUser, UserResource>(user);

      return new OkObjectResult(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] RegisterUserResource registerUserResource)
    {
      var language = Request.Headers["Accept-Language"].ToString();

      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(registerUserResource.Email);
        if (user != null)
        {
          ModelState.AddModelError("", "User is already exist");
          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }

        user = this.mapper.Map<RegisterUserResource, ApplicationUser>(registerUserResource);

        var role = await roleManager.FindByNameAsync(RolesEnum.User.ToString());
        if (role == null)
        {
          ModelState.AddModelError("", "Role not found");
          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }

        user.UserRoles.Add(new ApplicationUserRole() { RoleId = role.Id });

        user.EmailConfirmed = true;
        user.IsActive = true;
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        var result = await this.userManager.CreateAsync(user, registerUserResource.Password);
        if (result.Succeeded)
        {
          user = await this.userRepository.FindUserByIdAsync(user.Id);
          var userResource = this.mapper.Map<ApplicationUser, UserResource>(user);
          return new OkObjectResult(userResource);
        }

        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] string userId, [FromBody] UpdateUserResource updateUserResource)
    {
      var language = Request.Headers["Accept-Language"].ToString();

      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
          ModelState.AddModelError("", "User not found");
          return new NotFoundObjectResult(new NotFoundResource(ModelState));
        }

        this.mapper.Map<UpdateUserResource, ApplicationUser>(updateUserResource, user);

        user.UpdatedAt = DateTime.Now;

        var result = await this.userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
          user = await this.userRepository.FindUserByIdAsync(user.Id);
          var userResource = this.mapper.Map<ApplicationUser, UserResource>(user);

          return new OkObjectResult(userResource);
        }

        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserAsync([FromRoute] string userId)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
          ModelState.AddModelError("", "User not found");
          return new NotFoundObjectResult(new NotFoundResource(ModelState));
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.Now;

        var result = await this.userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
          user = await this.userRepository.FindUserByIdAsync(user.Id);
          var userResource = this.mapper.Map<ApplicationUser, UserResource>(user);

          return new OkObjectResult(userResource);
        }

        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPut("{userId}/Activate")]
    public async Task<IActionResult> ActivateUserAsync([FromRoute] string userId)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByIdAsync(userId);
        if (user == null)
        {
          ModelState.AddModelError("", "User not found");
          return new NotFoundObjectResult(new NotFoundResource(ModelState));
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.Now;

        var result = await this.userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
          user = await this.userRepository.FindUserByIdAsync(user.Id);
          var userResource = this.mapper.Map<ApplicationUser, UserResource>(user);

          return new OkObjectResult(userResource);
        }

        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError("", error.Description);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPost("AssignRole")]
    public async Task<IActionResult> AddUserToRoleAsync([FromQuery] string userId, [FromQuery] string roleId)
    {
      var user = await this.userManager.FindByIdAsync(userId);
      if (user == null)
      {
        ModelState.AddModelError("", "User not found");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      var role = await roleManager.FindByIdAsync(roleId);
      if (role == null)
      {
        ModelState.AddModelError("", "Role not found");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      if (await userManager.IsInRoleAsync(user, role.Name))
      {
        ModelState.AddModelError("", $"User is already added to role {role.Name}");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      var result = await userManager.AddToRoleAsync(user, role.Name);
      if (result.Succeeded)
      {
        await this.unitOfWork.CompleteAsync();

        return new OkObjectResult(new { message = $"User ({user.FirstName}) was added to role ({role.Name})" });
      }

      foreach (IdentityError error in result.Errors)
      {
        ModelState.AddModelError("", error.Description);
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpDelete("AssignRole")]
    public async Task<IActionResult> RemoveUserFromRoleAsync([FromQuery] string userId, [FromQuery] string roleId)
    {
      var user = await this.userManager.FindByIdAsync(userId);
      if (user == null)
      {
        ModelState.AddModelError("", "User not found");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      var role = await roleManager.FindByIdAsync(roleId);
      if (role == null)
      {
        ModelState.AddModelError("", "Role not found");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      if (!await userManager.IsInRoleAsync(user, role.Name))
      {
        ModelState.AddModelError("", $"User is already not in role {role.Name}");
        return new BadRequestObjectResult(new BadRequestResource(ModelState));
      }

      var result = await userManager.RemoveFromRoleAsync(user, role.Name);
      if (result.Succeeded)
      {
        await this.unitOfWork.CompleteAsync();

        return new OkObjectResult(new { message = $"User ({user.FirstName}) was removed from role ({role.Name})" });
      }

      foreach (IdentityError error in result.Errors)
      {
        ModelState.AddModelError("", error.Description);
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }
  }
}