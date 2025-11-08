using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MarcadorFaseIIApi.Data;
using MarcadorFaseIIApi.Models;

namespace MarcadorFaseIIApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemporadasController : ControllerBase
{
    private readonly MarcadorDbContext _context;

    public TemporadasController(MarcadorDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Temporada>>> GetTemporadas()
    {
        return await _context.Temporadas.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Temporada>> GetTemporada(int id)
    {
        var temporada = await _context.Temporadas.FindAsync(id);
        if (temporada == null) return NotFound();
        return temporada;
    }

    [HttpPost]
    public async Task<ActionResult<Temporada>> PostTemporada(Temporada temporada)
    {
        _context.Temporadas.Add(temporada);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTemporada), new { id = temporada.TemporadaId }, temporada);
    }
}