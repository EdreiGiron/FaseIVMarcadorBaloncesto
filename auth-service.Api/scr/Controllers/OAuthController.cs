using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Services.Interfaces;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("oauth")]
public class OAuthController(IAuthService auth) : ControllerBase
{
    // 1) Disparadores
    [HttpGet("google/login")]
    public IActionResult GoogleLogin()
        => Challenge(new AuthenticationProperties { RedirectUri = "/oauth/google/callback" }, "Google");

    [HttpGet("github/login")]
    public IActionResult GitHubLogin()
        => Challenge(new AuthenticationProperties { RedirectUri = "/oauth/github/callback" }, "GitHub");

    // 2) Callbacks
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
        => await HandleExternalCallback();

    [HttpGet("github/callback")]
    public async Task<IActionResult> GitHubCallback()
        => await HandleExternalCallback();

    private async Task<IActionResult> HandleExternalCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
            return Unauthorized(new { message = "OAuth error" });

        var principal = result.Principal;

        // Extraer email / nombre
        string? email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirst("email")?.Value
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        string? name = principal.Identity?.Name
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? email;

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "No se obtuvo email del proveedor" });

        var tokens = await auth.ExternalSignInAsync(email, name);
        await HttpContext.SignOutAsync("External");

        return tokens is null ? Problem("No se pudo crear usuario externo")
            : Ok(tokens);
    }
}
