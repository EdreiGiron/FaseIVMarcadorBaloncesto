using AuthService.Api.Models;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);
    Task RemoveAsync(Role role);
    Task<bool> IsInUseAsync(int roleId);
    Task SaveChangesAsync();
}