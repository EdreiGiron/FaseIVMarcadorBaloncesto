using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs;

namespace MarcadorFaseIIApi.Services.Interfaces
{
    /// <summary>
    /// Contrato de servicio para gestión de usuarios (CRUD y paginación).
    /// </summary>
    public interface IUserService
    {
        /// <summary>Devuelve un listado paginado con filtro de búsqueda.</summary>
        Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? search);

        /// <summary>Crea un usuario.</summary>
        Task<UserDto> CreateAsync(UserCreateDto dto);

        /// <summary>Actualiza un usuario por id.</summary>
        Task<UserDto> UpdateAsync(int id, UserUpdateDto dto);

        /// <summary>Elimina un usuario por id.</summary>
        Task DeleteAsync(int id);

        /// <summary>Devuelve todos los usuarios.</summary>
        Task<IEnumerable<UserDto>> GetAllAsync();

        /// <summary>Obtiene un usuario por id.</summary>
        Task<UserDto?> GetByIdAsync(int id);
    }
}
