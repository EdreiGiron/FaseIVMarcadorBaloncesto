using AuthService.Api.Models.DTOs;
using AuthService.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;


namespace AuthService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var res = await auth.AuthenticateAsync(dto);
        return res is null ? Unauthorized(new { message = "Credenciales inválidas" }) : Ok(res);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        var res = await auth.RefreshAsync(dto);
        return res is null ? Unauthorized(new { message = "Refresh inválido/expirado" }) : Ok(res);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var res = await auth.RegisterAsync(dto);
        return res is null ? BadRequest(new { message = "Username existente o rol inválido" }) : Ok(res);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string username)
    {
        await auth.LogoutAsync(username);
        return NoContent();
    }

}
