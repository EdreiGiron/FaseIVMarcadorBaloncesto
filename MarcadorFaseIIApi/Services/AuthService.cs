using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs;
using MarcadorFaseIIApi.Repositories.Interfaces;
using MarcadorFaseIIApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace MarcadorFaseIIApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshRepo,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshRepo = refreshRepo;
        _config = config;
    }

    /// <summary>
    /// Autentica a un usuario con credenciales (username/password), genera un JWT de acceso
    /// y emite/persiste un refresh token asociado al usuario.
    /// </summary>
    /// <param name="request">
    /// DTO con <see cref="LoginRequestDto.Username"/> y <see cref="LoginRequestDto.Password"/>.
    /// </param>
    /// <returns>
    /// <para>
    /// <see cref="LoginResponseDto"/> con <c>Token</c> (JWT de acceso), <c>RefreshToken</c>,
    /// <c>Username</c> y <c>Role</c> si las credenciales son válidas; de lo contrario <c>null</c>.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Flujo:
    /// 1) Busca el usuario y su rol (carga defensiva si faltara).
    /// 2) Verifica contraseña con BCrypt.
    /// 3) Genera JWT de acceso (tiempo corto, p. ej. 5 min).
    /// 4) Emite refresh token persistente (p. ej. 7 días).
    /// </remarks>
    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameWithRoleAsync(request.Username);
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return null;

        // Carga defensiva de rol
        if (user.Role == null)
        {
            var role = await _roleRepository.GetByIdAsync(user.RoleId);
            if (role != null) user.Role = role;
        }

        var access = GenerateJwtToken(user);
        var refresh = await IssueRefreshTokenAsync(user.Id);

        return new LoginResponseDto
        {
            Username = user.Username,
            Role = new RoleDto { Name = user.Role?.Name ?? "User" },
            Token = access,
            RefreshToken = refresh
        };
    }

    /// <summary>
    /// Renueva el JWT de acceso usando un refresh token válido y no revocado.
    /// Rota el refresh token (invalida el actual y emite uno nuevo).
    /// </summary>
    /// <param name="request">
    /// DTO con <see cref="RefreshRequestDto.RefreshToken"/>.
    /// </param>
    /// <returns>
    /// <see cref="LoginResponseDto"/> con nuevo <c>Token</c> (JWT) y <c>RefreshToken</c>,
    /// o <c>null</c> si el refresh token es inválido/expirado/revocado.
    /// </returns>
    /// <remarks>
    /// Seguridad:
    /// - Se valida que el refresh esté activo, no revocado y con <c>ExpiresAt</c> en el futuro.
    /// - Se marca el token anterior como revocado y se persiste el nuevo.
    /// </remarks>
    public async Task<LoginResponseDto?> RefreshAsync(RefreshRequestDto request)
    {
        var stored = await _refreshRepo.GetActiveByTokenAsync(request.RefreshToken);
        if (stored == null || stored.ExpiresAt <= DateTime.UtcNow || stored.IsRevoked)
            return null;

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user == null) return null;

        if (user.Role == null)
        {
            var role = await _roleRepository.GetByIdAsync(user.RoleId);
            if (role != null) user.Role = role;
        }

        // Rotar refresh
        stored.IsRevoked = true;

        var days = int.TryParse(_config["Jwt:RefreshDays"], out var d) ? d : 7;
        var newRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await _refreshRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            ReplacedByToken = stored.Token
        });

        var newAccess = GenerateJwtToken(user);
        await _refreshRepo.SaveChangesAsync();

        return new LoginResponseDto
        {
            Username = user.Username,
            Role = new RoleDto { Name = user.Role?.Name ?? "User" },
            Token = newAccess,
            RefreshToken = newRefresh
        };
    }
    /// <summary>
    /// Registra un nuevo usuario con rol asignado y contraseña hasheada (BCrypt).
    /// </summary>
    /// <param name="request">
    /// DTO con <c>Username</c>, <c>Password</c> y <c>RoleId</c>.
    /// </param>
    /// <returns>
    /// <see cref="RegisterResponseDto"/> con datos del usuario creado;
    /// <c>null</c> si el usuario ya existe o el rol no es válido.
    /// </returns>
    /// <remarks>
    /// Efectos:
    /// - Persiste el nuevo usuario con <c>Password</c> hasheado.
    /// - No devuelve tokens; el login se realiza en el endpoint de autenticación.
    /// </remarks>
    public async Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null) return null;

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null) return null;

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Password = hashedPassword,
            RoleId = role.Id
        };

        await _userRepository.AddAsync(user);

        return new RegisterResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            RoleName = role.Name,
            Message = "Usuario registrado correctamente"
        };
    }

    /// <summary>
    /// Emite y persiste un nuevo refresh token para un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="replacedToken">
    /// Token previo que se reemplaza (opcional, útil durante rotación).
    /// </param>
    /// <returns>Cadena aleatoria segura (Base64) del nuevo refresh token.</returns>
    /// <remarks>
    /// - La expiración se calcula con <c>Jwt:RefreshDays</c>.
    /// - Usa <see cref="RandomNumberGenerator"/> para generar el token.
    /// - Persiste la entidad <see cref="RefreshToken"/> con <c>ExpiresAt</c>.
    /// </remarks>
    private async Task<string> IssueRefreshTokenAsync(int userId, string? replacedToken = null)
    {
        var daysCfg = _config["Jwt:RefreshDays"];
        var days = int.TryParse(daysCfg, out var d) ? d : 7;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            IsRevoked = false,
            ReplacedByToken = replacedToken
        };

        await _refreshRepo.AddAsync(entity);
        await _refreshRepo.SaveChangesAsync();

        return token;
    }
    /// <summary>
    /// Genera un JWT de acceso firmando con HMAC-SHA256.
    /// </summary>
    /// <param name="user">Entidad de usuario con <c>Username</c> y <c>Role</c>.</param>
    /// <returns>Token JWT en formato compacto (string).</returns>
    /// <remarks>
    /// Claims:
    /// - <c>ClaimTypes.Name</c>: username
    /// - <c>ClaimTypes.Role</c>: nombre del rol (por defecto "User")
    /// Configuración:
    /// - <c>Jwt:Key</c>, <c>Jwt:Issuer</c>, <c>Jwt:Audience</c>, <c>Jwt:ExpiresInMinutes</c>.
    /// </remarks>
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roleName = user.Role?.Name ?? "User";
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName)
        };

        var minutes = int.TryParse(jwtSettings["ExpiresInMinutes"], out var m) ? m : 5;

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    //<summary>
    //Metodo Logout 
    //</summary>
    public async Task LogoutAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null) return;

        await _refreshRepo.RevokeAllForUserAsync(user.Id);
        await _refreshRepo.SaveChangesAsync();
    }
}
