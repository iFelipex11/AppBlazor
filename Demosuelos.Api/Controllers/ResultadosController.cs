using Demosuelos.Api.Data;
using Demosuelos.Api.Services;
using Demosuelos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ResultadosController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly EnsayoReglasService _reglasService;

    public ResultadosController(
        IDbContextFactory<AppDbContext> dbFactory,
        EnsayoReglasService reglasService)
    {
        _dbFactory = dbFactory;
        _reglasService = reglasService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResultadoParametro>>> Get()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var resultados = await db.ResultadosParametro
            .Include(x => x.ParametroEnsayo)
                .ThenInclude(p => p!.TipoEnsayo)
            .Include(x => x.EnsayoRealizado)
                .ThenInclude(e => e!.Muestra)
                    .ThenInclude(m => m!.PuntoMuestreo)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(resultados);
    }

    [HttpGet("por-ensayo/{ensayoId:int}")]
    public async Task<ActionResult<List<ResultadoParametro>>> GetPorEnsayo(int ensayoId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var resultados = await db.ResultadosParametro
            .Include(x => x.ParametroEnsayo)
            .Include(x => x.EnsayoRealizado)
                .ThenInclude(e => e!.TipoEnsayo)
            .Where(x => x.EnsayoRealizadoId == ensayoId)
            .OrderBy(x => x.ParametroEnsayo!.Nombre)
            .ToListAsync();

        return Ok(resultados);
    }

    [HttpGet("parametros-disponibles/{ensayoId:int}")]
    public async Task<ActionResult<List<ParametroEnsayo>>> GetParametrosDisponibles(int ensayoId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayo = await db.EnsayosRealizados
            .FirstOrDefaultAsync(x => x.Id == ensayoId);

        if (ensayo is null)
            return NotFound("El ensayo realizado no existe.");

        var parametros = await db.ParametrosEnsayo
            .Where(x => x.TipoEnsayoId == ensayo.TipoEnsayoId && !x.EsCalculado)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return Ok(parametros);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] ResultadoParametro resultado)
    {
        var registro = await _reglasService.RegistrarResultadoAsync(resultado);

        if (!registro.Exito)
            return BadRequest(registro.Mensaje);

        var validacion = await _reglasService.ValidarEnsayoAsync(resultado.EnsayoRealizadoId);

        return Ok(new
        {
            Mensaje = registro.Mensaje,
            Resultado = registro.Datos,
            Validacion = validacion
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] ResultadoParametro resultado)
    {
        if (id != resultado.Id)
            return BadRequest("El Id de la ruta no coincide con el del resultado.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var existente = await db.ResultadosParametro
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (existente is null)
            return NotFound("Resultado de parámetro no encontrado.");

        resultado.EnsayoRealizadoId = existente.EnsayoRealizadoId;
        resultado.ParametroEnsayoId = resultado.ParametroEnsayoId == 0
            ? existente.ParametroEnsayoId
            : resultado.ParametroEnsayoId;

        var registro = await _reglasService.RegistrarResultadoAsync(new ResultadoParametro
        {
            Id = id,
            EnsayoRealizadoId = resultado.EnsayoRealizadoId,
            ParametroEnsayoId = resultado.ParametroEnsayoId,
            Valor = resultado.Valor,
            Observacion = resultado.Observacion
        });

        if (!registro.Exito)
            return BadRequest(registro.Mensaje);

        var validacion = await _reglasService.ValidarEnsayoAsync(resultado.EnsayoRealizadoId);

        return Ok(new
        {
            Mensaje = "Resultado actualizado correctamente.",
            Resultado = registro.Datos,
            Validacion = validacion
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var resultado = await db.ResultadosParametro.FindAsync(id);
        if (resultado is null)
            return NotFound("Resultado de parámetro no encontrado.");

        var ensayoId = resultado.EnsayoRealizadoId;

        db.ResultadosParametro.Remove(resultado);
        await db.SaveChangesAsync();

        var validacion = await _reglasService.ValidarEnsayoAsync(ensayoId);

        return Ok(new
        {
            Mensaje = "Resultado eliminado correctamente.",
            Validacion = validacion
        });
    }
}
