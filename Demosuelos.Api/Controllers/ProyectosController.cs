using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProyectosController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProyectosController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<Proyecto>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var proyectos = await db.Proyectos
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(proyectos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Proyecto>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var proyecto = await db.Proyectos.FindAsync(id);

        if (proyecto is null)
            return NotFound("Proyecto no encontrado.");

        return Ok(proyecto);
    }

    [HttpPost]
    public async Task<ActionResult<Proyecto>> Post([FromBody] Proyecto proyecto)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync();

        return Ok(proyecto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] Proyecto proyecto)
    {
        if (id != proyecto.Id)
            return BadRequest("El Id de la ruta no coincide con el del proyecto.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.Proyectos.FindAsync(id);

        if (existente is null)
            return NotFound("Proyecto no encontrado.");

        existente.Nombre = proyecto.Nombre;
        existente.Cliente = proyecto.Cliente;
        existente.Ubicacion = proyecto.Ubicacion;
        existente.Estado = proyecto.Estado;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var proyecto = await db.Proyectos.FindAsync(id);

        if (proyecto is null)
            return NotFound("Proyecto no encontrado.");

        db.Proyectos.Remove(proyecto);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
