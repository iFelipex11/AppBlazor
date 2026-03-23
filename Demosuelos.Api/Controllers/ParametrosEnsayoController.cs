using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ParametrosEnsayoController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ParametrosEnsayoController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParametroEnsayo>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var parametros = await db.ParametrosEnsayo
            .Include(x => x.TipoEnsayo)
            .OrderBy(x => x.TipoEnsayo!.Codigo)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        return Ok(parametros);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParametroEnsayo>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var parametro = await db.ParametrosEnsayo
            .Include(x => x.TipoEnsayo)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (parametro is null)
            return NotFound("Parámetro de ensayo no encontrado.");

        return Ok(parametro);
    }

    [HttpPost]
    public async Task<ActionResult<ParametroEnsayo>> Post([FromBody] ParametroEnsayo parametro)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        parametro.Nombre = parametro.Nombre?.Trim() ?? string.Empty;
        parametro.Unidad = string.IsNullOrWhiteSpace(parametro.Unidad) ? null : parametro.Unidad.Trim();

        if (string.IsNullOrWhiteSpace(parametro.Nombre))
            return BadRequest("Debes ingresar el nombre del parámetro.");

        var tipoExiste = await db.TiposEnsayo.AnyAsync(x => x.Id == parametro.TipoEnsayoId);
        if (!tipoExiste)
            return BadRequest("El tipo de ensayo seleccionado no existe.");

        if (parametro.MinReferencial.HasValue && parametro.MaxReferencial.HasValue &&
            parametro.MinReferencial > parametro.MaxReferencial)
        {
            return BadRequest("El mínimo referencial no puede ser mayor que el máximo referencial.");
        }

        var duplicado = await db.ParametrosEnsayo
            .AnyAsync(x => x.TipoEnsayoId == parametro.TipoEnsayoId && x.Nombre == parametro.Nombre);

        if (duplicado)
            return BadRequest("Ya existe un parámetro con ese nombre para el tipo de ensayo seleccionado.");

        db.ParametrosEnsayo.Add(parametro);
        await db.SaveChangesAsync();

        var creado = await db.ParametrosEnsayo
            .Include(x => x.TipoEnsayo)
            .FirstAsync(x => x.Id == parametro.Id);

        return Ok(creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] ParametroEnsayo parametro)
    {
        if (id != parametro.Id)
            return BadRequest("El Id de la ruta no coincide con el del parámetro.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.ParametrosEnsayo.FindAsync(id);
        if (existente is null)
            return NotFound("Parámetro de ensayo no encontrado.");

        parametro.Nombre = parametro.Nombre?.Trim() ?? string.Empty;
        parametro.Unidad = string.IsNullOrWhiteSpace(parametro.Unidad) ? null : parametro.Unidad.Trim();

        if (string.IsNullOrWhiteSpace(parametro.Nombre))
            return BadRequest("Debes ingresar el nombre del parámetro.");

        var tipoExiste = await db.TiposEnsayo.AnyAsync(x => x.Id == parametro.TipoEnsayoId);
        if (!tipoExiste)
            return BadRequest("El tipo de ensayo seleccionado no existe.");

        if (parametro.MinReferencial.HasValue && parametro.MaxReferencial.HasValue &&
            parametro.MinReferencial > parametro.MaxReferencial)
        {
            return BadRequest("El mínimo referencial no puede ser mayor que el máximo referencial.");
        }

        var duplicado = await db.ParametrosEnsayo
            .AnyAsync(x => x.Id != id &&
                           x.TipoEnsayoId == parametro.TipoEnsayoId &&
                           x.Nombre == parametro.Nombre);

        if (duplicado)
            return BadRequest("Ya existe otro parámetro con ese nombre para el tipo de ensayo seleccionado.");

        existente.TipoEnsayoId = parametro.TipoEnsayoId;
        existente.Nombre = parametro.Nombre;
        existente.Unidad = parametro.Unidad;
        existente.Requerido = parametro.Requerido;
        existente.EsCalculado = parametro.EsCalculado;
        existente.MinReferencial = parametro.MinReferencial;
        existente.MaxReferencial = parametro.MaxReferencial;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var parametro = await db.ParametrosEnsayo.FindAsync(id);
        if (parametro is null)
            return NotFound("Parámetro de ensayo no encontrado.");

        var tieneResultados = await db.ResultadosParametro.AnyAsync(x => x.ParametroEnsayoId == id);
        if (tieneResultados)
            return BadRequest("No se puede eliminar el parámetro porque tiene resultados asociados.");

        db.ParametrosEnsayo.Remove(parametro);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
