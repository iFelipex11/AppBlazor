using Demosuelos.Api.Services;
using Demosuelos.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InterpretacionController : ControllerBase
{
    private readonly EnsayoReglasService _reglasService;
    private readonly GeminiInterpretacionService _geminiService;

    public InterpretacionController(
        EnsayoReglasService reglasService,
        GeminiInterpretacionService geminiService)
    {
        _reglasService = reglasService;
        _geminiService = geminiService;
    }

    [HttpPost("muestra/{muestraId:int}")]
    public async Task<ActionResult<InterpretacionIaResponse>> InterpretarMuestra(int muestraId)
    {
        var resumen = await _reglasService.ObtenerResumenMuestraAsync(muestraId);

        if (resumen is null)
        {
            return NotFound(new InterpretacionIaResponse
            {
                Exito = false,
                Mensaje = "La muestra no existe."
            });
        }

        var texto = await _geminiService.ExplicarResultadosAsync(
            resumen.CodigoMuestra,
            resumen.HumedadNatural,
            resumen.LimiteLiquido,
            resumen.LimitePlastico,
            resumen.IndicePlasticidad);

        return Ok(new InterpretacionIaResponse
        {
            Exito = true,
            Mensaje = "Interpretación generada correctamente.",
            Interpretacion = texto
        });
    }
}
