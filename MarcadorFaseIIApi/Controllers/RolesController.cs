using Microsoft.AspNetCore.Mvc;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Repositories.Interfaces;

namespace MarcadorFaseIIApi.Controllers
{
    /// <summary>
    /// CRUD de roles (básico, directo al repositorio).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;

        /// <summary>
        /// Crea el controlador de roles con su repositorio.
        /// </summary>
        /// <param name="roleRepo">Repositorio de roles.</param>
        public RolesController(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// Lista todos los roles.
        /// </summary>
        /// <returns>200 con colección de <see cref="Role"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _roleRepo.GetAllAsync());

        /// <summary>
        /// Obtiene un rol por id.
        /// </summary>
        /// <param name="id">Id del rol.</param>
        /// <returns>200 con rol; 404 si no existe.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = $"Rol con id {id} no encontrado" });

            return Ok(role);
        }

        /// <summary>
        /// Crea un nuevo rol.
        /// </summary>
        /// <param name="role">Entidad de rol.</param>
        /// <returns>200 con rol creado.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            await _roleRepo.AddAsync(role);
            return Ok(role);
        }

        /// <summary>
        /// Actualiza un rol existente.
        /// </summary>
        /// <param name="id">Id del rol.</param>
        /// <param name="updatedRole">Entidad con el nombre actualizado.</param>
        /// <returns>200 con rol; 404 si no existe.</returns>
        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Role updatedRole)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = $"Rol con id {id} no encontrado" });

            role.Name = updatedRole.Name;
            await _roleRepo.UpdateAsync(role);

            return Ok(role);
        }

        /// <summary>
        /// Elimina un rol por id.
        /// </summary>
        /// <param name="id">Id del rol.</param>
        /// <returns>204 si se elimina; 404 si no existe.</returns>
        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleRepo.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { message = $"Rol con id {id} no encontrado" });

            await _roleRepo.DeleteAsync(role);
            return NoContent(); // 204 sin contenido
        }
    }
}
