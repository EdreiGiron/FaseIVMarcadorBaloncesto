using MarcadorFaseIIApi.Models.DTOs;

namespace MarcadorFaseIIApi.Services.Interfaces;

/// <summary>
/// Contrato del servicio de autenticación (login, refresh y registro).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Autentica y devuelve tokens (access + refresh).
    /// </summary>
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);

    /// <summary>
    /// Renueva el access token usando un refresh token vigente.
    /// </summary>
    Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto request);

    /// <summary>
    /// Registra un usuario nuevo con rol.
    /// </summary>
    Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request);
    /// <summary>
    /// Hace logout de la aplicacion y elimina token 
    /// </summary>
    Task LogoutAsync(string username);
}
