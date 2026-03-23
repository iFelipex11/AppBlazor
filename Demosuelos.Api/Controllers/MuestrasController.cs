using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
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
            .OrderByDescending(x => x.FechaMuestreo)
            .ThenByDescending(x => x.FechaRecepcion)
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

        muestra.CodigoMuestra = muestra.CodigoMuestra?.Trim() ?? string.Empty;
        muestra.TipoMuestra = string.IsNullOrWhiteSpace(muestra.TipoMuestra) ? null : muestra.TipoMuestra.Trim();
        muestra.EstadoMuestra = string.IsNullOrWhiteSpace(muestra.EstadoMuestra) ? "Registrada" : muestra.EstadoMuestra.Trim();
        muestra.Observaciones = string.IsNullOrWhiteSpace(muestra.Observaciones) ? null : muestra.Observaciones.Trim();

        if (string.IsNullOrWhiteSpace(muestra.CodigoMuestra))
            return BadRequest("Debes ingresar el código de la muestra.");

        var puntoExiste = await db.PuntosMuestreo.AnyAsync(x => x.Id == muestra.PuntoMuestreoId);
        if (!puntoExiste)
            return BadRequest("El punto de muestreo seleccionado no existe.");

        var duplicada = await db.Muestras.AnyAsync(x => x.CodigoMuestra == muestra.CodigoMuestra);
        if (duplicada)
            return BadRequest("Ya existe una muestra con ese código.");

        if (muestra.ProfundidadInicial.HasValue && muestra.ProfundidadInicial < 0)
            return BadRequest("La profundidad inicial no puede ser negativa.");

        if (muestra.ProfundidadFinal.HasValue && muestra.ProfundidadFinal < 0)
            return BadRequest("La profundidad final no puede ser negativa.");

        if (muestra.ProfundidadInicial.HasValue &&
            muestra.ProfundidadFinal.HasValue &&
            muestra.ProfundidadInicial > muestra.ProfundidadFinal)
        {
            return BadRequest("La profundidad inicial no puede ser mayor que la profundidad final.");
        }

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

        muestra.CodigoMuestra = muestra.CodigoMuestra?.Trim() ?? string.Empty;
        muestra.TipoMuestra = string.IsNullOrWhiteSpace(muestra.TipoMuestra) ? null : muestra.TipoMuestra.Trim();
        muestra.EstadoMuestra = string.IsNullOrWhiteSpace(muestra.EstadoMuestra) ? "Registrada" : muestra.EstadoMuestra.Trim();
        muestra.Observaciones = string.IsNullOrWhiteSpace(muestra.Observaciones) ? null : muestra.Observaciones.Trim();

        if (string.IsNullOrWhiteSpace(muestra.CodigoMuestra))
            return BadRequest("Debes ingresar el código de la muestra.");

        var puntoExiste = await db.PuntosMuestreo.AnyAsync(x => x.Id == muestra.PuntoMuestreoId);
        if (!puntoExiste)
            return BadRequest("El punto de muestreo seleccionado no existe.");

        var duplicada = await db.Muestras
            .AnyAsync(x => x.Id != id && x.CodigoMuestra == muestra.CodigoMuestra);

        if (duplicada)
            return BadRequest("Ya existe otra muestra con ese código.");

        if (muestra.ProfundidadInicial.HasValue && muestra.ProfundidadInicial < 0)
            return BadRequest("La profundidad inicial no puede ser negativa.");

        if (muestra.ProfundidadFinal.HasValue && muestra.ProfundidadFinal < 0)
            return BadRequest("La profundidad final no puede ser negativa.");

        if (muestra.ProfundidadInicial.HasValue &&
            muestra.ProfundidadFinal.HasValue &&
            muestra.ProfundidadInicial > muestra.ProfundidadFinal)
        {
            return BadRequest("La profundidad inicial no puede ser mayor que la profundidad final.");
        }

        existente.PuntoMuestreoId = muestra.PuntoMuestreoId;
        existente.CodigoMuestra = muestra.CodigoMuestra;
        existente.FechaRecepcion = muestra.FechaRecepcion;
        existente.FechaMuestreo = muestra.FechaMuestreo;
        existente.ProfundidadInicial = muestra.ProfundidadInicial;
        existente.ProfundidadFinal = muestra.ProfundidadFinal;
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
