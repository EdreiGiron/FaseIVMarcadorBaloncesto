using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Services.Interfaces;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("oauth")]
public sealed class OAuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IConfiguration _cfg;

    public OAuthController(IAuthService auth, IConfiguration cfg)
    {
        _auth = auth;
        _cfg = cfg;
    }

    // --- Launch endpoints---
    [HttpGet("google")]
    [HttpGet("google/login")]
    public IActionResult GoogleLogin()
        => StartChallenge("Google", "google");

    [HttpGet("github")]
    [HttpGet("github/login")]
    public IActionResult GitHubLogin()
        => StartChallenge("GitHub", "github");

    private IActionResult StartChallenge(string scheme, string provider)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalCallback), new { provider })!
        };
        return Challenge(props, scheme);
    }

    // --- Callback unificado ---
    [HttpGet("{provider:regex(^google|github$)}/callback")]
    public async Task<IActionResult> ExternalCallback(string provider)
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
            return BadRequest("External auth failed.");

        var p = result.Principal;
        var email = p.FindFirstValue(ClaimTypes.Email)
                    ?? p.FindFirst("email")?.Value
                    ?? p.Identity?.Name;
        var name = p.Identity?.Name
                    ?? p.FindFirstValue(ClaimTypes.Name)
                    ?? email;

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Provider did not return an email.");

        var login = await _auth.ExternalSignInAsync(email!, name);
        await HttpContext.SignOutAsync("External");

        if (login is null) return Problem("Could not sign in/create user.");

        // Redirige al front con los tokens
        var front = _cfg["OAuth:FrontCallback"] ?? "http://localhost:4200/auth/callback";
        var url = $"{front}" +
                  $"?accessToken={Uri.EscapeDataString(login.Token)}" +
                  $"&refreshToken={Uri.EscapeDataString(login.RefreshToken)}" +
                  $"&name={Uri.EscapeDataString(login.Username)}" +
                  $"&role={Uri.EscapeDataString(login.Role?.Name ?? "USER")}";

        return Redirect(url);
    }
}
