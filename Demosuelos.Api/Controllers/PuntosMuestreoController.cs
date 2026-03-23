using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PuntosMuestreoController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public PuntosMuestreoController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<PuntoMuestreo>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var puntos = await db.PuntosMuestreo
            .Include(x => x.Proyecto)
            .OrderBy(x => x.ProyectoId)
            .ThenBy(x => x.Codigo)
            .ToListAsync();

        return Ok(puntos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PuntoMuestreo>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var punto = await db.PuntosMuestreo
            .Include(x => x.Proyecto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (punto is null)
            return NotFound("Punto de muestreo no encontrado.");

        return Ok(punto);
    }

    [HttpPost]
    public async Task<ActionResult<PuntoMuestreo>> Post([FromBody] PuntoMuestreo punto)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        punto.Codigo = punto.Codigo?.Trim() ?? string.Empty;
        punto.Sector = string.IsNullOrWhiteSpace(punto.Sector) ? null : punto.Sector.Trim();
        punto.Descripcion = string.IsNullOrWhiteSpace(punto.Descripcion) ? null : punto.Descripcion.Trim();
        punto.CoordenadaX = string.IsNullOrWhiteSpace(punto.CoordenadaX) ? null : punto.CoordenadaX.Trim();
        punto.CoordenadaY = string.IsNullOrWhiteSpace(punto.CoordenadaY) ? null : punto.CoordenadaY.Trim();

        if (string.IsNullOrWhiteSpace(punto.Codigo))
            return BadRequest("Debes ingresar el código del punto de muestreo.");

        var proyectoExiste = await db.Proyectos.AnyAsync(x => x.Id == punto.ProyectoId);
        if (!proyectoExiste)
            return BadRequest("El proyecto seleccionado no existe.");

        var duplicado = await db.PuntosMuestreo
            .AnyAsync(x => x.ProyectoId == punto.ProyectoId && x.Codigo == punto.Codigo);

        if (duplicado)
            return BadRequest("Ya existe un punto de muestreo con ese código en el proyecto seleccionado.");

        db.PuntosMuestreo.Add(punto);
        await db.SaveChangesAsync();

        var creado = await db.PuntosMuestreo
            .Include(x => x.Proyecto)
            .FirstAsync(x => x.Id == punto.Id);

        return Ok(creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] PuntoMuestreo punto)
    {
        if (id != punto.Id)
            return BadRequest("El Id de la ruta no coincide con el del punto de muestreo.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.PuntosMuestreo.FindAsync(id);
        if (existente is null)
            return NotFound("Punto de muestreo no encontrado.");

        punto.Codigo = punto.Codigo?.Trim() ?? string.Empty;
        punto.Sector = string.IsNullOrWhiteSpace(punto.Sector) ? null : punto.Sector.Trim();
        punto.Descripcion = string.IsNullOrWhiteSpace(punto.Descripcion) ? null : punto.Descripcion.Trim();
        punto.CoordenadaX = string.IsNullOrWhiteSpace(punto.CoordenadaX) ? null : punto.CoordenadaX.Trim();
        punto.CoordenadaY = string.IsNullOrWhiteSpace(punto.CoordenadaY) ? null : punto.CoordenadaY.Trim();

        if (string.IsNullOrWhiteSpace(punto.Codigo))
            return BadRequest("Debes ingresar el código del punto de muestreo.");

        var proyectoExiste = await db.Proyectos.AnyAsync(x => x.Id == punto.ProyectoId);
        if (!proyectoExiste)
            return BadRequest("El proyecto seleccionado no existe.");

        var duplicado = await db.PuntosMuestreo
            .AnyAsync(x => x.Id != id &&
                           x.ProyectoId == punto.ProyectoId &&
                           x.Codigo == punto.Codigo);

        if (duplicado)
            return BadRequest("Ya existe otro punto de muestreo con ese código en el proyecto seleccionado.");

        existente.ProyectoId = punto.ProyectoId;
        existente.Codigo = punto.Codigo;
        existente.Sector = punto.Sector;
        existente.Descripcion = punto.Descripcion;
        existente.CoordenadaX = punto.CoordenadaX;
        existente.CoordenadaY = punto.CoordenadaY;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var punto = await db.PuntosMuestreo.FindAsync(id);
        if (punto is null)
            return NotFound("Punto de muestreo no encontrado.");

        var tieneMuestras = await db.Muestras.AnyAsync(x => x.PuntoMuestreoId == id);
        if (tieneMuestras)
            return BadRequest("No se puede eliminar el punto de muestreo porque tiene muestras asociadas.");

        db.PuntosMuestreo.Remove(punto);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
