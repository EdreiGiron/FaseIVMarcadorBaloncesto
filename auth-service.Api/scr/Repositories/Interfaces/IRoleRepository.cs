using AuthService.Api.Models;
using AuthService.Api.Models.DTOs;

namespace AuthService.Api.Repositories.Interfaces
{
    /// <summary>
    /// Contrato de persistencia para operaciones sobre <see cref="Role"/>.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Obtiene un rol por su identificador.
        /// </summary>
        /// <param name="id">Id del rol.</param>
        /// <returns>La entidad encontrada o <c>null</c> si no existe.</returns>
        Task<Role?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un rol por su nombre.
        /// </summary>
        /// <param name="name">Nombre del rol.</param>
        /// <returns>La entidad encontrada o <c>null</c> si no existe.</returns>
        Task<Role?> GetByNameAsync(string name);

        /// <summary>
        /// Devuelve el listado completo de roles.
        /// </summary>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// Agrega un nuevo rol y guarda cambios.
        /// </summary>
        /// <param name="role">Entidad de rol a crear.</param>
        Task AddAsync(Role role);

        /// <summary>
        /// Actualiza un rol existente y guarda cambios.
        /// </summary>
        /// <param name="role">Entidad con los datos actualizados.</param>
        Task UpdateAsync(Role role);

        /// <summary>
        /// Elimina un rol y guarda cambios.
        /// </summary>
        /// <param name="role">Entidad a eliminar.</param>
        Task DeleteAsync(Role role);
    }
}
