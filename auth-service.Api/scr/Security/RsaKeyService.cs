using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AuthService.Api.Security;

public sealed class RsaKeyService
{
    public RsaSecurityKey Key { get; }
    public string KeyId { get; }
    public JsonWebKey Jwk { get; }

    public RsaKeyService(IConfiguration cfg)
    {
        var kid = cfg["Jwt:KeyId"] ?? "auth-rsa-kid";
        var privPath = cfg["Jwt:PrivateKeyPemPath"];
        var pubPath  = cfg["Jwt:PublicKeyPemPath"];

        if (string.IsNullOrWhiteSpace(privPath) || string.IsNullOrWhiteSpace(pubPath))
            throw new InvalidOperationException("Faltan rutas a llaves RSA en appsettings.");

        var privatePem = File.ReadAllText(privPath);
        var publicPem  = File.ReadAllText(pubPath);

        var rsa = RSA.Create();
        rsa.ImportFromPem(privatePem);

        var rsaPub = RSA.Create();
        rsaPub.ImportFromPem(publicPem);
        var rsaParams = rsaPub.ExportParameters(false);

        var key = new RsaSecurityKey(rsa) { KeyId = kid };
        Key = key;
        KeyId = kid;

        string B64Url(byte[] x) => Base64UrlEncoder.Encode(x);
        Jwk = new JsonWebKey
        {
            Kty = "RSA",
            Kid = kid,
            Use = "sig",
            Alg = SecurityAlgorithms.RsaSha256,
            N = B64Url(rsaParams.Modulus!),
            E = B64Url(rsaParams.Exponent!)
        };
    }
}
