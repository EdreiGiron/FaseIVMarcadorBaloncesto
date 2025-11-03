using AuthService.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController(RsaKeyService rsa) : ControllerBase
{
    [HttpGet] public IActionResult Get([FromServices] RsaKeyService svc) => Ok(new { keys = new[] { svc.Jwk } });
}
