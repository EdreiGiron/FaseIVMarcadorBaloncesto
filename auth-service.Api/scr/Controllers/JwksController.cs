using Microsoft.AspNetCore.Mvc;
using AuthService.Api.Security;

namespace AuthService.Api.Controllers;

[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController : ControllerBase
{
    private readonly RsaKeyService _rsa;
    public JwksController(RsaKeyService rsa) => _rsa = rsa;

    [HttpGet]
    public IActionResult Get() => Ok(new { keys = new[] { _rsa.Jwk } });
}
