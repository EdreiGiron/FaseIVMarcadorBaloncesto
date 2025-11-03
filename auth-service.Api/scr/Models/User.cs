namespace AuthService.Api.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
}
