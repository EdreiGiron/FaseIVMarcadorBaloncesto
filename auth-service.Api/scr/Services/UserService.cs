using AuthService.Api.Repositories.Interfaces;

namespace AuthService.Api.Services;

public class UserService(IUserRepository repo) : Interfaces.IUserService
{
    public async Task<IEnumerable<object>> GetAllAsync()
        => (await repo.GetAllAsync()).Select(u => new { u.Id, u.Username, Role = u.RoleId });
}
