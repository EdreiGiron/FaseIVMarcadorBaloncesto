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

    //OAuth
    [ApiController]
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IConfiguration _cfg;

        public OAuthController(IAuthService auth, IConfiguration cfg)
        { _auth = auth; _cfg = cfg; }

        //Google
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authRes = await HttpContext.AuthenticateAsync("External");
            if (!authRes.Succeeded) return BadRequest("External auth failed");

            var email = authRes.Principal?.FindFirstValue(ClaimTypes.Email)
                        ?? authRes.Properties?.GetTokenValue("email");
            var name = authRes.Principal?.Identity?.Name;

            var login = await _auth.ExternalSignInAsync(email!, name);
            var front = _cfg["OAuth:FrontCallback"] ?? "http://localhost:4200/auth/callback";

            var url = QueryHelpers.AddQueryString(front, new Dictionary<string, string?>
            {
                ["token"] = login!.Token,
                ["refreshToken"] = login.RefreshToken,
                ["username"] = login.Username,
                ["role"] = login.Role?.Name ?? "USER"
            });
            return Redirect(url);
        }

        //Github
        [HttpGet("github/callback")]
        public async Task<IActionResult> GithubCallback()
        {
            var authRes = await HttpContext.AuthenticateAsync("External");
            if (!authRes.Succeeded) return BadRequest("External auth failed");

            // GitHub: email puede venir en token/claim
            var email = authRes.Principal?.FindFirstValue(ClaimTypes.Email)
                        ?? authRes.Properties?.GetTokenValue("email");
            var name = authRes.Principal?.Identity?.Name;

            var login = await _auth.ExternalSignInAsync(email!, name);
            var front = _cfg["OAuth:FrontCallback"] ?? "http://localhost:4200/auth/callback";

            var url = QueryHelpers.AddQueryString(front, new Dictionary<string, string?>
            {
                ["token"] = login!.Token,
                ["refreshToken"] = login.RefreshToken,
                ["username"] = login.Username,
                ["role"] = login.Role?.Name ?? "USER"
            });
            return Redirect(url);
        }
    }
}
