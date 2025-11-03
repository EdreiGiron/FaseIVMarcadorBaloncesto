using AuthService.Api.Repositories.Interfaces;
using AuthService.Api.Data;
using AuthService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Repositories;

public class UserRepository(AuthDbContext ctx) : IUserRepository
{
    private readonly AuthDbContext _ctx = ctx;

    public Task<User?> GetByIdAsync(int id) => _ctx.Users.FindAsync(id).AsTask();
    public Task<User?> GetByUsernameAsync(string username) =>
        _ctx.Users.FirstOrDefaultAsync(u => u.Username == username);
    public Task<User?> GetByUsernameWithRoleAsync(string username) =>
        _ctx.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);
    public async Task<IEnumerable<User>> GetAllAsync() => await _ctx.Users.ToListAsync();
    public async Task AddAsync(User user) { _ctx.Users.Add(user); await _ctx.SaveChangesAsync(); }
    public async Task UpdateAsync(User user) { _ctx.Users.Update(user); await _ctx.SaveChangesAsync(); }
    public async Task DeleteAsync(User user) { _ctx.Users.Remove(user); await _ctx.SaveChangesAsync(); }
}