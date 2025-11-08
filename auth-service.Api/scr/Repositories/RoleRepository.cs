using Microsoft.EntityFrameworkCore;
using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Data;
using AuthService.Api.Models.DTOs;
using AuthService.Api.Models;

namespace AuthService.Api.Repositories;

public class RoleRepository(AuthDbContext ctx) : IRoleRepository
{
    private readonly AuthDbContext _ctx = ctx;

    public Task<Role?> GetByIdAsync(int id) => _ctx.Roles.FindAsync(id).AsTask();
    public Task<Role?> GetByNameAsync(string name) => _ctx.Roles.FirstOrDefaultAsync(r => r.Name == name);
    public async Task<IEnumerable<Role>> GetAllAsync() => await _ctx.Roles.ToListAsync();
    public async Task AddAsync(Role role) { _ctx.Roles.Add(role); await _ctx.SaveChangesAsync(); }
    public async Task UpdateAsync(Role role) { _ctx.Roles.Update(role); await _ctx.SaveChangesAsync(); }
    public async Task DeleteAsync(Role role) { _ctx.Roles.Remove(role); await _ctx.SaveChangesAsync(); }

    public async Task RemoveAsync(Role role)
    {
        _ctx.Roles.Remove(role);
        await _ctx.SaveChangesAsync();
    }

    public Task<bool> IsInUseAsync(int roleId)
        => _ctx.Users.AnyAsync(u => u.RoleId == roleId);

    public Task SaveChangesAsync()
        => _ctx.SaveChangesAsync();
}

