using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using IdentityApi.Constants;
using IdentityApi.Controllers.Resources;
using IdentityApi.Controllers.Resources.ApiError;
using IdentityApi.Core.Contracts;
using IdentityApi.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : Controller
  {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IRefreshTokenRepository refreshTokenRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IConfiguration config;
    public AuthController(UserManager<ApplicationUser> userManager,
                          RoleManager<ApplicationRole> roleManager,
                          SignInManager<ApplicationUser> signInManager,
                          IRefreshTokenRepository refreshTokenRepository,
                          IUnitOfWork unitOfWork,
                          IMapper mapper,
                          IConfiguration config)
    {
      this.userManager = userManager;
      this.roleManager = roleManager;
      this.signInManager = signInManager;
      this.refreshTokenRepository = refreshTokenRepository;
      this.unitOfWork = unitOfWork;
      this.mapper = mapper;
      this.config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUserResource userResource)
    {
      if (ModelState.IsValid)
      {
        var user = mapper.Map<RegisterUserResource, ApplicationUser>(userResource);

        var role = await roleManager.FindByNameAsync(RolesEnum.User.ToString());
        if (role == null)
        {
          ModelState.AddModelError("", "Role not found");
          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }

        user.UserRoles.Add(new ApplicationUserRole() { RoleId = role.Id });

        user.IsActive = true;
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        var result = await this.userManager.CreateAsync(user, userResource.Password);

        if (!result.Succeeded)
        {
          foreach (IdentityError error in result.Errors)
          {
            ModelState.AddModelError("", error.Description);
          }

          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }
        else
        {
          var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
          var callbackUrl = Url.Action(
              "ConfirmEmail",
              "Auth",
              values: new EmailConfirmationResource { UserId = user.Id, Token = token },
              protocol: Request.Scheme);

          await this.unitOfWork.CompleteAsync();

          return new OkObjectResult(new { callbackUrl = callbackUrl });
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginUserResource loginUserResource)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(loginUserResource.Email);

        if (user == null || !await this.userManager.CheckPasswordAsync(user, loginUserResource.Password))
        {
          ModelState.AddModelError("", "Email and password does not match");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }
        else if (!user.EmailConfirmed && await this.userManager.CheckPasswordAsync(user, loginUserResource.Password))
        {
          ModelState.AddModelError("", "Email not confirmed yet");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }
        else if (!user.IsActive)
        {
          ModelState.AddModelError("Auth", "User has disactivated");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }

        var result = await this.signInManager.CheckPasswordSignInAsync(user, loginUserResource.Password, false);

        if (!result.Succeeded)
        {
          ModelState.AddModelError("", "Email and password does not match");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }
        else
        {
          var newRefreshToken = new RefreshToken
          {
            UserId = user.Id,
            AccessToken = await GenerateAccessTokenAsync(user),
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now
          };

          this.refreshTokenRepository.Add(newRefreshToken);

          await this.unitOfWork.CompleteAsync();

          var response = new TokenResource()
          {
            AccessToken = newRefreshToken.AccessToken,
            RefreshToken = newRefreshToken.Token
          };

          return new OkObjectResult(response);
        }
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenResource resource)
    {
      if (ModelState.IsValid)
      {
        var refreshToken = await this.refreshTokenRepository.GetRefreshTokenAsync(new RefreshToken { AccessToken = resource.AccessToken, Token = resource.RefreshToken });
        if (refreshToken == null)
        {
          return Unauthorized();
        }
        if (!await this.signInManager.CanSignInAsync(refreshToken.User))
        {
          return Unauthorized();
        }

        var newRefreshToken = new RefreshToken
        {
          UserId = refreshToken.User.Id,
          AccessToken = await GenerateAccessTokenAsync(refreshToken.User),
          Token = Guid.NewGuid().ToString(),
          CreatedAt = DateTime.Now
        };
        this.refreshTokenRepository.Add(newRefreshToken);
        await this.unitOfWork.CompleteAsync();

        var response = new TokenResource()
        {
          AccessToken = newRefreshToken.AccessToken,
          RefreshToken = newRefreshToken.Token
        };

        return new OkObjectResult(response);
      }

      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] EmailConfirmationResource emailConfirmationResource)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByIdAsync(emailConfirmationResource.UserId);
        if (user == null)
        {
          ModelState.AddModelError("", "Email and password does not match");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }

        var result = await this.userManager.ConfirmEmailAsync(user, emailConfirmationResource.Token);
        if (!result.Succeeded)
        {
          foreach (IdentityError error in result.Errors)
          {
            ModelState.AddModelError("", error.Description);
          }

          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }
        else
        {
          return new OkObjectResult(new { message = "Email has confirmed successfully" });
        }
      }

      // If we got this far, something failed, redisplay form
      ModelState.AddModelError("", "Failed to confirm email");
      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpPost("ResendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] LoginUserResource loginUserResource)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(loginUserResource.Email);

        if (user == null || !await this.userManager.CheckPasswordAsync(user, loginUserResource.Password))
        {
          ModelState.AddModelError("", "Email and password does not match");
          return new UnauthorizedObjectResult(new UnAuthorizedResource(ModelState));
        }
        else if (!user.EmailConfirmed)
        {
          var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
          var callbackUrl = Url.Action(
              "ConfirmEmail",
              "Auth",
              values: new EmailConfirmationResource { UserId = user.Id, Token = token },
              protocol: Request.Scheme);

          return new OkObjectResult(new { callbackUrl = callbackUrl });
        }
        else
        {
          ModelState.AddModelError("", "Email is already confirmed");
          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }
      }

      // If we got this far, something failed, redisplay form
      ModelState.AddModelError("", "Failed to resend confirmation email");
      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    [HttpGet("SendPasswordResetLink")]
    public async Task<IActionResult> SendPasswordResetLink([FromQuery] string email)
    {
      var user = await this.userManager.FindByEmailAsync(email);
      if (user == null || !(this.userManager.IsEmailConfirmedAsync(user).Result))
      {
        ModelState.AddModelError("", $"User ({email}) does not exist");
        return new NotFoundObjectResult(new NotFoundResource(ModelState));
      }

      var token = await this.userManager.GeneratePasswordResetTokenAsync(user);

      var resetLink = Url.Action("ResetPassword",
                      "Auth", new { token = token },
                       protocol: HttpContext.Request.Scheme);

      // await _emailSender.SendEmailAsync(userEmail, "Reset your password",
      //  $"Please follow this  <a href='{HtmlEncoder.Default.Encode(resetLink)}'>link </a> to reset your password.");

      return new OkObjectResult(new { resetPassowardLink = resetLink });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] ResetPasswordResource resource)
    {
      if (ModelState.IsValid)
      {
        var user = await this.userManager.FindByEmailAsync(resource.Email);

        IdentityResult result = await this.userManager.ResetPasswordAsync(user, token, resource.Password);
        if (!result.Succeeded)
        {
          foreach (IdentityError error in result.Errors)
          {
            ModelState.AddModelError("", error.Description);
          }

          return new BadRequestObjectResult(new BadRequestResource(ModelState));
        }
        else
        {
          return new OkObjectResult(new { message = "reset password has done successfully" });
        }
      }

      // If we got this far, something failed, redisplay form
      ModelState.AddModelError("", "Failed reset Password");
      return new BadRequestObjectResult(new BadRequestResource(ModelState));
    }

    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
      List<string> roles = (List<string>)await this.userManager.GetRolesAsync(user);

      List<Claim> identityClaims = (List<Claim>)await this.userManager.GetClaimsAsync(user);

      List<Claim> claims = new List<Claim>
      {
          new Claim(JwtRegisteredClaimNames.Sub, user.Id),
          new Claim(JwtRegisteredClaimNames.Email, user.Email),
          new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
          new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      };

      claims.AddRange(identityClaims);

      foreach (string role in roles)
      {
        claims.Add(new Claim("UserClaims", role));
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      foreach (var claim in identityClaims)
      {
        claims.Add(new Claim("UserClaims", claim.Value));
      }

      DateTime tokenEndTime = DateTime.Now;
      tokenEndTime = tokenEndTime.AddDays(1);

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["Token:Key"]));
      var audiences = this.config.GetSection("Token:Audience").Get<List<string>>();

      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      // Add multi audience
      foreach (var aud in audiences)
      {
        claims.Add(new Claim("aud", aud));
      }

      JwtSecurityToken token = new JwtSecurityToken(
          issuer: this.config["Token:Issuer"],
          claims: claims,
          expires: tokenEndTime,
          signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

  }
}