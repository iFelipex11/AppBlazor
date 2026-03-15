using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnsayosController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public EnsayosController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<EnsayoRealizado>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayos = await db.EnsayosRealizados
            .Include(x => x.Muestra)
                .ThenInclude(m => m!.PuntoMuestreo)
            .Include(x => x.TipoEnsayo)
            .OrderByDescending(x => x.FechaEnsayo)
            .ThenBy(x => x.Id)
            .ToListAsync();

        return Ok(ensayos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EnsayoRealizado>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayo = await db.EnsayosRealizados
            .Include(x => x.Muestra)
                .ThenInclude(m => m!.PuntoMuestreo)
            .Include(x => x.TipoEnsayo)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ensayo is null)
            return NotFound("Ensayo realizado no encontrado.");

        return Ok(ensayo);
    }

    [HttpPost]
    public async Task<ActionResult<EnsayoRealizado>> Post([FromBody] EnsayoRealizado ensayo)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var muestra = await db.Muestras.FindAsync(ensayo.MuestraId);
        if (muestra is null)
            return BadRequest("La muestra seleccionada no existe.");

        var tipoEnsayo = await db.TiposEnsayo.FindAsync(ensayo.TipoEnsayoId);
        if (tipoEnsayo is null)
            return BadRequest("El tipo de ensayo seleccionado no existe.");

        if (!tipoEnsayo.Activo)
            return BadRequest("El tipo de ensayo seleccionado está inactivo.");

        var duplicado = await db.EnsayosRealizados
            .AnyAsync(x => x.MuestraId == ensayo.MuestraId && x.TipoEnsayoId == ensayo.TipoEnsayoId);

        if (duplicado)
            return BadRequest("Ya existe ese tipo de ensayo para la muestra seleccionada.");

        if (string.IsNullOrWhiteSpace(ensayo.Responsable))
            return BadRequest("Debes ingresar el responsable del ensayo.");

        if (string.IsNullOrWhiteSpace(ensayo.Estado))
            ensayo.Estado = "Pendiente";

        db.EnsayosRealizados.Add(ensayo);
        await db.SaveChangesAsync();

        var creado = await db.EnsayosRealizados
            .Include(x => x.Muestra)
                .ThenInclude(m => m!.PuntoMuestreo)
            .Include(x => x.TipoEnsayo)
            .FirstAsync(x => x.Id == ensayo.Id);

        return Ok(creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] EnsayoRealizado ensayo)
    {
        if (id != ensayo.Id)
            return BadRequest("El Id de la ruta no coincide con el del ensayo.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.EnsayosRealizados.FindAsync(id);
        if (existente is null)
            return NotFound("Ensayo realizado no encontrado.");

        var muestra = await db.Muestras.FindAsync(ensayo.MuestraId);
        if (muestra is null)
            return BadRequest("La muestra seleccionada no existe.");

        var tipoEnsayo = await db.TiposEnsayo.FindAsync(ensayo.TipoEnsayoId);
        if (tipoEnsayo is null)
            return BadRequest("El tipo de ensayo seleccionado no existe.");

        if (!tipoEnsayo.Activo)
            return BadRequest("El tipo de ensayo seleccionado está inactivo.");

        var duplicado = await db.EnsayosRealizados
            .AnyAsync(x => x.Id != id &&
                           x.MuestraId == ensayo.MuestraId &&
                           x.TipoEnsayoId == ensayo.TipoEnsayoId);

        if (duplicado)
            return BadRequest("Ya existe otro ensayo con ese tipo para la muestra seleccionada.");

        if (string.IsNullOrWhiteSpace(ensayo.Responsable))
            return BadRequest("Debes ingresar el responsable del ensayo.");

        existente.MuestraId = ensayo.MuestraId;
        existente.TipoEnsayoId = ensayo.TipoEnsayoId;
        existente.FechaEnsayo = ensayo.FechaEnsayo;
        existente.Responsable = ensayo.Responsable;
        existente.Estado = string.IsNullOrWhiteSpace(ensayo.Estado) ? "Pendiente" : ensayo.Estado;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayo = await db.EnsayosRealizados.FindAsync(id);
        if (ensayo is null)
            return NotFound("Ensayo realizado no encontrado.");

        var tieneResultados = await db.ResultadosParametro.AnyAsync(x => x.EnsayoRealizadoId == id);
        if (tieneResultados)
            return BadRequest("No se puede eliminar el ensayo porque tiene resultados asociados.");

        db.EnsayosRealizados.Remove(ensayo);
        await db.SaveChangesAsync();

        return NoContent();
    }
}