using AuthService.Api.Models.DTOs;

namespace AuthService.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
    Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto request);
    Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request);
    Task LogoutAsync(string username);
    Task<LoginResponseDto?> ExternalSignInAsync(string email, string? displayName);

}
