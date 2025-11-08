using AuthService.Api.Data;
using AuthService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Controllers;

public record UsuarioDto(int Id, string Username, string RoleName);
public record CreateUserDto(string Username, string Password, int RoleId);
public record UpdateUserDto(string? Username, string? Password, int? RoleId);

[ApiController]
[Route("auth/users")]
[Authorize(Roles = "ADMIN")]
public sealed class UsersController(AuthDbContext db) : ControllerBase
{
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<UsuarioDto>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 10 : pageSize;

        var q = db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(u => u.Username.ToLower().Contains(term));
        }

        var total = await q.CountAsync();
        var items = await q.OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UsuarioDto(u.Id, u.Username, u.Role!.Name))
            .ToListAsync();

        return Ok(PagedResult<UsuarioDto>.Create(items, total, page, pageSize));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Username y password son requeridos." });

        var exists = await db.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists) return Conflict(new { message = "El username ya existe." });

        var role = await db.Roles.FindAsync(dto.RoleId);
        if (role is null) return BadRequest(new { message = "Rol inválido." });

        var user = new User
        {
            Username = dto.Username.Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPaged), new { id = user.Id },
            new UsuarioDto(user.Id, user.Username, role.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Username))
        {
            var newUser = dto.Username.Trim();
            var exists = await db.Users.AnyAsync(u => u.Username == newUser && u.Id != id);
            if (exists) return Conflict(new { message = "Otro usuario ya tiene ese username." });
            user.Username = newUser;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (dto.RoleId.HasValue)
        {
            var role = await db.Roles.FindAsync(dto.RoleId.Value);
            if (role is null) return BadRequest(new { message = "Rol inválido." });
            user.RoleId = role.Id;
            user.Role = role;
        }

        await db.SaveChangesAsync();
        return Ok(new UsuarioDto(user.Id, user.Username, user.Role!.Name));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        var tokens = db.RefreshTokens.Where(t => t.UserId == id);
        db.RefreshTokens.RemoveRange(tokens);

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
