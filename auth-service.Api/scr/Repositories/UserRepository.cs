using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using AuthService.Api.Data;
using AuthService.Api.Models;
using AuthService.Api.Models.DTOs;
using AuthService.Api.Repositories.Interfaces;

namespace AuthService.Api.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _db;
        public UserRepository(AuthDbContext db) => _db = db;

        public Task<User?> GetByIdAsync(int id) =>
            _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByUsernameAsync(string username) =>
            _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task<User?> GetByUsernameWithRoleAsync(string username) =>
            _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _db.Users.AsNoTracking().Include(u => u.Role).OrderBy(u => u.Username).ToListAsync();

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        // La interfaz define ambos: DeleteAsync y RemoveAsync.
        public async Task DeleteAsync(User user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(User user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public Task<bool> ExistsUsernameAsync(string username, int? excludeUserId = null) =>
            _db.Users.AnyAsync(u => u.Username == username && (excludeUserId == null || u.Id != excludeUserId));

        public async Task<(IReadOnlyList<UsuarioDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var q = _db.Users.AsNoTracking().Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.Username.Contains(search));

            var total = await q.CountAsync();

            var items = await q.OrderBy(u => u.Username)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .Select(u => new UsuarioDto
                               {
                                   Id = u.Id,
                                   Username = u.Username,
                                   RoleName = u.Role != null ? u.Role.Name : string.Empty
                               })
                               .ToListAsync();

            return (items, total);
        }
    }
}
