namespace AuthService.Api.Models.DTOs;

public record LoginRequestDto(string Username, string Password);
public record RefreshRequestDto(string RefreshToken);
public record RegisterRequestDto(string Username, string Password, int RoleId);
public record RoleDto(string Name);

public class LoginResponseDto
{
    public required string Username { get; set; }
    public required RoleDto Role { get; set; }
    public required string Token { get; set; }          // access token (JWT RS256)
    public required string RefreshToken { get; set; }   // refresh token
}

public class RegisterResponseDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string RoleName { get; set; }
    public string Message { get; set; } = "OK";
}
