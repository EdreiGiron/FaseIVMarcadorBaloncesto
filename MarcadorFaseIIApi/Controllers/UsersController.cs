using Microsoft.AspNetCore.Mvc;
using MarcadorFaseIIApi.Models;
using MarcadorFaseIIApi.Models.DTOs;
using MarcadorFaseIIApi.Services.Interfaces;

namespace MarcadorFaseIIApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/users/paged?page=1&pageSize=10&search=alex
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _userService.GetPagedAsync(page, pageSize, search);
            return Ok(result);
        }

        // POST /api/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var user = await _userService.CreateAsync(dto);
            return Ok(user);
        }

        // PUT /api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            var updated = await _userService.UpdateAsync(id, dto);
            return Ok(updated);
        }

        // DELETE /api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}
