using AuthService.Api.Models;

namespace AuthService.Api.Repositories.Interfaces
{
    /// <summary>
    /// Contrato de persistencia para operaciones sobre <see cref="User"/>.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="id">Id del usuario.</param>
        /// <returns>La entidad encontrada o <c>null</c> si no existe.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns>La entidad encontrada o <c>null</c> si no existe.</returns>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario, incluyendo la navegación de rol.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns>La entidad con su rol, o <c>null</c> si no existe.</returns>
        Task<User?> GetByUsernameWithRoleAsync(string username);

        /// <summary>
        /// Devuelve el listado completo de usuarios.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Agrega un nuevo usuario y guarda cambios.
        /// </summary>
        /// <param name="user">Entidad de usuario a crear.</param>
        Task AddAsync(User user);

        /// <summary>
        /// Actualiza un usuario y guarda cambios.
        /// </summary>
        /// <param name="user">Entidad con los datos actualizados.</param>
        Task UpdateAsync(User user);

        /// <summary>
        /// Elimina un usuario y guarda cambios.
        /// </summary>
        /// <param name="user">Entidad a eliminar.</param>
        Task DeleteAsync(User user);
    }
}
