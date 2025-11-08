using AuthService.Api.Models;
using AuthService.Api.Models.DTOs;

namespace AuthService.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameWithRoleAsync(string username);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);

    Task<bool> ExistsUsernameAsync(string username, int? excludeUserId = null);
    Task RemoveAsync(User user);
    Task SaveChangesAsync();

    Task<(IReadOnlyList<UsuarioDto> Items, int Total)>
        GetPagedAsync(int page, int pageSize, string? search);
}