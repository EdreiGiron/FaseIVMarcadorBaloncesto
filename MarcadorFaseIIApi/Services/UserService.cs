using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs;
using MarcadorFaseIIApi.Repositories.Interfaces;
using MarcadorFaseIIApi.Services.Interfaces;
using MarcadorFaseIIApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcadorFaseIIApi.Services
{
    /// <summary>
    /// Servicio de usuarios: CRUD, consultas y paginación.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly MarcadorDbContext _context;
        private readonly IUserRepository _userRepo;

        /// <summary>
        /// Crea una instancia del servicio de usuarios.
        /// </summary>
        public UserService(MarcadorDbContext context, IUserRepository userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        // ✅ Obtener todos los usuarios
        /// <summary>
        /// Devuelve todos los usuarios con su rol (si existe).
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty
                })
                .ToListAsync();
        }

        // ✅ Buscar usuario por ID
        /// <summary>
        /// Obtiene un usuario por id y proyecta a DTO.
        /// </summary>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty
                })
                .FirstOrDefaultAsync();
        }

        // ✅ Buscar usuario por Username
        /// <summary>
        /// Obtiene un usuario por nombre de usuario (incluye rol).
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        // ✅ Crear usuario
        /// <summary>
        /// Crea un usuario con contraseña hasheada y retorna DTO.
        /// </summary>
        public async Task<UserDto> CreateAsync(UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                RoleName = (await _context.Roles.FindAsync(user.RoleId))?.Name ?? string.Empty
            };
        }

        // ✅ Actualizar usuario
        /// <summary>
        /// Actualiza datos de un usuario (username, password y rol).
        /// </summary>
        public async Task<UserDto> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            // Actualizar propiedades
            user.Username = dto.Username ?? user.Username;

            // Actualizar password si se envía
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            // Actualizar RoleId si viene en el DTO
            if (dto.RoleId.HasValue)
            {
                user.RoleId = dto.RoleId.Value;
            }

            await _userRepo.UpdateAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                RoleName = user.Role?.Name ?? string.Empty
            };
        }

        // Eliminar usuario
        /// <summary>
        /// Elimina un usuario por id si existe.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        // Listado paginado
        /// <summary>
        /// Devuelve usuarios paginados con filtro opcional por nombre de usuario.
        /// </summary>
        public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? search)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Username.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role != null ? u.Role.Name : string.Empty
                })
                .ToListAsync();

            return new PagedResult<UserDto>
            {
                Items = items,
                Total = total
            };
        }
    }
}
