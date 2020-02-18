namespace IdentityApi.Controllers.Resources
{
  public class UserQueryResource
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string RoleId { get; set; }

    public string SortBy { get; set; }

    public bool? IsSortAscending { get; set; }

    public int Page { get; set; }

    public byte PageSize { get; set; }

    public bool? IsActive { get; set; }
  }
}