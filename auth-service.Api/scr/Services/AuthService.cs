using AuthService.Api.Models;
using AuthService.Api.Models.DTOs;
using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Api.Security;

namespace AuthService.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refresh;
    private readonly IConfiguration _cfg;
    private readonly RsaKeyService _rsa;

    public AuthService(IUserRepository u, IRoleRepository r, IRefreshTokenRepository rt, IConfiguration cfg, RsaKeyService rsa)
    { _users = u; _roles = r; _refresh = rt; _cfg = cfg; _rsa = rsa; }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto req)
    {
        var user = await _users.GetByUsernameWithRoleAsync(req.Username);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.Password)) return null;

        user.Role ??= await _roles.GetByIdAsync(user.RoleId);

        var token = GenerateJwt(user);
        var refresh = await IssueRefresh(user.Id);

        return new LoginResponseDto
        {
            Username = user.Username,
            Role = new RoleDto { Name = user.Role?.Name ?? "User" },
            Token = token,
            RefreshToken = refresh
        };
    }

    public async Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto req)
    {
        var stored = await _refresh.GetActiveByTokenAsync(req.RefreshToken);
        if (stored == null || stored.ExpiresAt <= DateTime.UtcNow || stored.IsRevoked) return null;

        var user = await _users.GetByIdAsync(stored.UserId);
        if (user == null) return null;

        user.Role ??= await _roles.GetByIdAsync(user.RoleId);

        stored.IsRevoked = true;
        var newRefresh = await IssueRefresh(user.Id, stored.Token);
        var newAccess = GenerateJwt(user);
        await _refresh.SaveChangesAsync();

        return new LoginResponseDto
        {
            Username = user.Username,
            Role = new RoleDto { Name = user.Role?.Name ?? "User" },
            Token = newAccess,
            RefreshToken = newRefresh
        };
    }

    public async Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto req)
    {
        var exists = await _users.GetByUsernameAsync(req.Username);
        if (exists != null) return null;
        var role = await _roles.GetByIdAsync(req.RoleId);
        if (role == null) return null;

        var user = new User
        {
            Username = req.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            RoleId = role.Id
        };
        await _users.AddAsync(user);

        return new RegisterResponseDto { Id = user.Id, Username = user.Username, RoleName = role.Name, Message = "OK" };
    }

    public async Task LogoutAsync(string username)
    {
        var u = await _users.GetByUsernameAsync(username);
        if (u == null) return;
        await _refresh.RevokeAllForUserAsync(u.Id);
        await _refresh.SaveChangesAsync();
    }

    private async Task<string> IssueRefresh(int userId, string? replaced = null)
    {
        var days = int.TryParse(_cfg["Jwt:RefreshDays"], out var d) ? d : 7;
        var val = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        await _refresh.AddAsync(new RefreshToken
        {
            UserId = userId,
            Token = val,
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            IsRevoked = false,
            ReplacedByToken = replaced
        });
        await _refresh.SaveChangesAsync();
        return val;
    }

    private string GenerateJwt(User u)
    {
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        var minutes = int.TryParse(_cfg["Jwt:ExpiresInMinutes"], out var m) ? m : 5;

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new Claim(ClaimTypes.Name, u.Username),
            new Claim(ClaimTypes.Role, u.Role?.Name ?? "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(_rsa.Key, SecurityAlgorithms.RsaSha256);
        var token = new JwtSecurityToken(
            issuer: issuer, audience: audience, claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );
        token.Header["kid"] = _rsa.KeyId;
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
