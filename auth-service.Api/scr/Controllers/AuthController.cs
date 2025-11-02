using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Models.DTOs;
using AuthService.Api.Services.Interfaces;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var res = await _auth.AuthenticateAsync(dto);
        return res is null ? Unauthorized(new { message = "Credenciales inválidas" }) : Ok(res);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        var res = await _auth.RefreshAsync(dto);
        return res is null ? Unauthorized(new { message = "Refresh inválido/expirado" }) : Ok(res);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var res = await _auth.RegisterAsync(dto);
        return res is null ? BadRequest(new { message = "Username existente o rol inválido" }) : Ok(res);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] UsernameDto dto)
    {
        await _auth.LogoutAsync(dto.Username);
        return NoContent();
    }
}

public record UsernameDto(string Username);
