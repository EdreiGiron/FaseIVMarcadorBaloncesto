using Microsoft.EntityFrameworkCore;
using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Data;
using AuthService.Api.Models.DTOs;
using AuthService.Api.Models;

namespace AuthService.Api.Repositories;

/// <summary>
/// Implementación de <see cref="IRoleRepository"/> con EF Core.
/// Provee operaciones CRUD básicas para <see cref="Role"/>.
/// </summary>
public class RoleRepository(MarcadorDbContext context) : IRoleRepository
{
    private readonly MarcadorDbContext _context = context;

    /// <summary>
    /// Agrega el role a la persona
    /// </summary>
    /// 
    public Task<Role> AddRoleAsync(RoleDto role)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Buscar role por nombre 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    /// <summary>
    /// Devuelve todos los roles existentes.
    /// </summary>
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    /// <summary>
    /// Obtiene un rol por id.
    /// </summary>
    /// <param name="id">Id del rol.</param>
    public async Task<Role?> GetByIdAsync(int id) =>
        await _context.Roles.FindAsync(id);

    /// <summary>
    /// Crea un nuevo rol y guarda cambios.
    /// </summary>
    /// <param name="role">Entidad de rol a persistir.</param>
    public async Task AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza un rol y guarda cambios.
    /// </summary>
    /// <param name="role">Entidad con los datos actualizados.</param>
    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un rol y guarda cambios.
    /// </summary>
    /// <param name="role">Entidad a eliminar.</param>
    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

}
