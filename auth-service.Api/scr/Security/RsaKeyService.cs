using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthService.Api.Security;

public sealed class RsaKeyService
{
    public RsaSecurityKey Key { get; }          
    public RsaSecurityKey PublicKey { get; }        
    public RSA PublicRsa { get; }                   
    public string KeyId { get; }
    public JsonWebKey Jwk { get; }

    public RsaKeyService(IConfiguration cfg)
    {
        var kid = cfg["Jwt:KeyId"] ?? "auth-rsa-kid-1";
        var privPath = cfg["Jwt:PrivateKeyPemPath"];
        var pubPath = cfg["Jwt:PublicKeyPemPath"];
        if (string.IsNullOrWhiteSpace(privPath) || string.IsNullOrWhiteSpace(pubPath))
            throw new InvalidOperationException("Faltan rutas de llaves RSA en appsettings.");

        var privatePem = File.ReadAllText(privPath);
        var publicPem = File.ReadAllText(pubPath);

        var rsa = RSA.Create(); rsa.ImportFromPem(privatePem);
        var rsaPub = RSA.Create(); rsaPub.ImportFromPem(publicPem);
        var p = rsaPub.ExportParameters(false);

        Key = new RsaSecurityKey(rsa) { KeyId = kid };
        PublicKey = new RsaSecurityKey(rsaPub) { KeyId = kid };
        PublicRsa = rsaPub;
        KeyId = kid;

        string B64Url(byte[] x) => Base64UrlEncoder.Encode(x);
        Jwk = new JsonWebKey
        {
            Kty = "RSA",
            Kid = kid,
            Use = "sig",
            Alg = SecurityAlgorithms.RsaSha256, // "RS256"
            N = B64Url(p.Modulus!),
            E = B64Url(p.Exponent!)
        };
    }
}
