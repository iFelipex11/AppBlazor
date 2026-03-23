using Demosuelos.Api.Data;
using Demosuelos.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ResumenController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly EnsayoReglasService _reglasService;

    public ResumenController(
        IDbContextFactory<AppDbContext> dbFactory,
        EnsayoReglasService reglasService)
    {
        _dbFactory = dbFactory;
        _reglasService = reglasService;
    }

    [HttpGet("muestra/{muestraId:int}")]
    public async Task<ActionResult> GetResumenMuestra(int muestraId)
    {
        var resumen = await _reglasService.ObtenerResumenMuestraAsync(muestraId);

        if (resumen is null)
            return NotFound("La muestra no existe.");

        return Ok(resumen);
    }

    [HttpPost("revalidar/{muestraId:int}")]
    public async Task<ActionResult> RevalidarMuestra(int muestraId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var muestraExiste = await db.Muestras.AnyAsync(x => x.Id == muestraId);
        if (!muestraExiste)
            return NotFound("La muestra no existe.");

        var ensayosIds = await db.EnsayosRealizados
            .Where(x => x.MuestraId == muestraId)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var ensayoId in ensayosIds)
        {
            await _reglasService.ValidarEnsayoAsync(ensayoId);
        }

        var resumen = await _reglasService.ObtenerResumenMuestraAsync(muestraId);

        return Ok(new
        {
            Mensaje = "Resumen recalculado correctamente.",
            Resumen = resumen
        });
    }
}
