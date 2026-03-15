using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MuestrasController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public MuestrasController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<Muestra>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var muestras = await db.Muestras
            .Include(x => x.PuntoMuestreo)
            .ThenInclude(p => p!.Proyecto)
            .OrderByDescending(x => x.FechaRecepcion)
            .ThenBy(x => x.CodigoMuestra)
            .ToListAsync();

        return Ok(muestras);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Muestra>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var muestra = await db.Muestras
            .Include(x => x.PuntoMuestreo)
            .ThenInclude(p => p!.Proyecto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (muestra is null)
            return NotFound("Muestra no encontrada.");

        return Ok(muestra);
    }

    [HttpPost]
    public async Task<ActionResult<Muestra>> Post([FromBody] Muestra muestra)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var puntoExiste = await db.PuntosMuestreo.AnyAsync(x => x.Id == muestra.PuntoMuestreoId);
        if (!puntoExiste)
            return BadRequest("El punto de muestreo seleccionado no existe.");

        var duplicada = await db.Muestras.AnyAsync(x => x.CodigoMuestra == muestra.CodigoMuestra);
        if (duplicada)
            return BadRequest("Ya existe una muestra con ese código.");

        db.Muestras.Add(muestra);
        await db.SaveChangesAsync();

        var creada = await db.Muestras
            .Include(x => x.PuntoMuestreo)
            .ThenInclude(p => p!.Proyecto)
            .FirstAsync(x => x.Id == muestra.Id);

        return Ok(creada);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] Muestra muestra)
    {
        if (id != muestra.Id)
            return BadRequest("El Id de la ruta no coincide con el de la muestra.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.Muestras.FindAsync(id);
        if (existente is null)
            return NotFound("Muestra no encontrada.");

        var puntoExiste = await db.PuntosMuestreo.AnyAsync(x => x.Id == muestra.PuntoMuestreoId);
        if (!puntoExiste)
            return BadRequest("El punto de muestreo seleccionado no existe.");

        var duplicada = await db.Muestras
            .AnyAsync(x => x.Id != id && x.CodigoMuestra == muestra.CodigoMuestra);

        if (duplicada)
            return BadRequest("Ya existe otra muestra con ese código.");

        existente.PuntoMuestreoId = muestra.PuntoMuestreoId;
        existente.CodigoMuestra = muestra.CodigoMuestra;
        existente.FechaRecepcion = muestra.FechaRecepcion;
        existente.TipoMuestra = muestra.TipoMuestra;
        existente.EstadoMuestra = muestra.EstadoMuestra;
        existente.Observaciones = muestra.Observaciones;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var muestra = await db.Muestras.FindAsync(id);
        if (muestra is null)
            return NotFound("Muestra no encontrada.");

        var tieneEnsayos = await db.EnsayosRealizados.AnyAsync(x => x.MuestraId == id);
        if (tieneEnsayos)
            return BadRequest("No se puede eliminar la muestra porque tiene ensayos asociados.");

        db.Muestras.Remove(muestra);
        await db.SaveChangesAsync();

        return NoContent();
    }
}