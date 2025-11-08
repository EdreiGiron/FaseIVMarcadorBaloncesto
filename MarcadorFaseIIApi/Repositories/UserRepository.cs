using MarcadorFaseIIApi.Repositories.Interfaces;
using MarcadorFaseIIApi.Data;
using MarcadorFaseIIApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarcadorFaseIIApi.Repositories;

/// <summary>
/// Implementación de <see cref="IUserRepository"/> con EF Core.
/// Provee operaciones CRUD básicas y búsquedas por username (con/sin rol).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly MarcadorDbContext _context;

    public UserRepository(MarcadorDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un usuario por su identificador.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    /// <returns>El usuario encontrado o <c>null</c> si no existe.</returns>
    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id); // Clave primaria -> FindAsync

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario.
    /// </summary>
    /// <param name="username">Nombre de usuario.</param>
    /// <returns>Entidad o <c>null</c> si no existe.</returns>
    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    /// <summary>
    /// Obtiene un usuario y su rol por nombre de usuario.
    /// </summary>
    /// <param name="username">Nombre de usuario.</param>
    /// <returns>Entidad con navegación de rol o <c>null</c> si no existe.</returns>
    public async Task<User?> GetByUsernameWithRoleAsync(string username) =>
        await _context.Users
                      .Include(u => u.Role)
                      .FirstOrDefaultAsync(u => u.Username == username);

    /// <summary>
    /// Devuelve todos los usuarios.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    /// <summary>
    /// Crea un nuevo usuario y guarda cambios.
    /// </summary>
    /// <param name="user">Entidad de usuario.</param>
    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza un usuario y guarda cambios.
    /// </summary>
    /// <param name="user">Entidad con datos actualizados.</param>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un usuario y guarda cambios.
    /// </summary>
    /// <param name="user">Entidad a eliminar.</param>
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
