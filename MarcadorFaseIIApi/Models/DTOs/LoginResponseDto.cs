namespace MarcadorFaseIIApi.Models.DTOs;

public class LoginResponseDto
{
        public string Username { get; set; } = default!;
        public RoleDto Role { get; set; } = default!;
        public string Token { get; set; } = default!;          // access token (JWT)
        public string RefreshToken { get; set; } = default!;
}
