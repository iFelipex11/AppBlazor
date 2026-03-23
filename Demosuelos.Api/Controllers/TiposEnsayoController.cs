using Demosuelos.Api.Data;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TiposEnsayoController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public TiposEnsayoController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoEnsayo>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var tipos = await db.TiposEnsayo
            .OrderBy(x => x.Codigo)
            .ToListAsync();

        return Ok(tipos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TipoEnsayo>> GetById(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var tipo = await db.TiposEnsayo.FindAsync(id);

        if (tipo is null)
            return NotFound("Tipo de ensayo no encontrado.");

        return Ok(tipo);
    }

    [HttpPost]
    public async Task<ActionResult<TipoEnsayo>> Post([FromBody] TipoEnsayo tipo)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var codigoExiste = await db.TiposEnsayo
            .AnyAsync(x => x.Codigo == tipo.Codigo);

        if (codigoExiste)
            return BadRequest("Ya existe un tipo de ensayo con ese código.");

        db.TiposEnsayo.Add(tipo);
        await db.SaveChangesAsync();

        return Ok(tipo);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] TipoEnsayo tipo)
    {
        if (id != tipo.Id)
            return BadRequest("El Id de la ruta no coincide con el del tipo de ensayo.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.TiposEnsayo.FindAsync(id);

        if (existente is null)
            return NotFound("Tipo de ensayo no encontrado.");

        var duplicado = await db.TiposEnsayo
            .AnyAsync(x => x.Id != id && x.Codigo == tipo.Codigo);

        if (duplicado)
            return BadRequest("Ya existe otro tipo de ensayo con ese código.");

        existente.Codigo = tipo.Codigo;
        existente.Nombre = tipo.Nombre;
        existente.Descripcion = tipo.Descripcion;
        existente.Activo = tipo.Activo;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var tipo = await db.TiposEnsayo.FindAsync(id);

        if (tipo is null)
            return NotFound("Tipo de ensayo no encontrado.");

        var tieneParametros = await db.ParametrosEnsayo.AnyAsync(x => x.TipoEnsayoId == id);
        if (tieneParametros)
            return BadRequest("No se puede eliminar el tipo de ensayo porque tiene parámetros asociados.");

        var tieneEnsayos = await db.EnsayosRealizados.AnyAsync(x => x.TipoEnsayoId == id);
        if (tieneEnsayos)
            return BadRequest("No se puede eliminar el tipo de ensayo porque tiene ensayos realizados asociados.");

        db.TiposEnsayo.Remove(tipo);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
