using AuthService.Api.Data;
using AuthService.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("auth/roles")]
public sealed class RolesController(AuthDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        => Ok(await db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync());

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<Role>> Create([FromBody] Role req)
    {
        var name = (req?.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Nombre requerido" });

        var exists = await db.Roles.AnyAsync(r => r.Name == name);
        if (exists) return Conflict(new { message = "El rol ya existe" });

        var role = new Role { Name = name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = role.Id }, role);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await db.Roles.FindAsync(id);
        if (role is null) return NotFound();

        var inUse = await db.Users.AnyAsync(u => u.RoleId == id);
        if (inUse) return BadRequest(new { message = "No se puede eliminar: hay usuarios con este rol." });

        db.Roles.Remove(role);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
