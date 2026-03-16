using Demosuelos.Api.Data;
using Demosuelos.Models;
using Demosuelos.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Demosuelos.Api.Services;

public class EnsayoReglasService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public EnsayoReglasService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ResultadoOperacion<ResultadoParametro>> RegistrarResultadoAsync(ResultadoParametro entrada)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayo = await db.EnsayosRealizados
            .Include(x => x.Muestra)
            .Include(x => x.TipoEnsayo)
            .FirstOrDefaultAsync(x => x.Id == entrada.EnsayoRealizadoId);

        if (ensayo is null)
        {
            return new ResultadoOperacion<ResultadoParametro>
            {
                Exito = false,
                Mensaje = "El ensayo realizado no existe."
            };
        }

        if (ensayo.Muestra is not null &&
            string.Equals(ensayo.Muestra.EstadoMuestra, "Cerrada", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoOperacion<ResultadoParametro>
            {
                Exito = false,
                Mensaje = "La muestra está cerrada y no permite registrar resultados."
            };
        }

        var parametro = await db.ParametrosEnsayo
            .FirstOrDefaultAsync(x => x.Id == entrada.ParametroEnsayoId);

        if (parametro is null)
        {
            return new ResultadoOperacion<ResultadoParametro>
            {
                Exito = false,
                Mensaje = "El parámetro de ensayo no existe."
            };
        }

        if (parametro.TipoEnsayoId != ensayo.TipoEnsayoId)
        {
            return new ResultadoOperacion<ResultadoParametro>
            {
                Exito = false,
                Mensaje = "El parámetro no pertenece al tipo de ensayo seleccionado."
            };
        }

        var existente = await db.ResultadosParametro
            .FirstOrDefaultAsync(x =>
                x.EnsayoRealizadoId == entrada.EnsayoRealizadoId &&
                x.ParametroEnsayoId == entrada.ParametroEnsayoId);

        var cumpleRango = CalcularCumpleRango(entrada.Valor, parametro.MinReferencial, parametro.MaxReferencial);
        var observacionTecnica = ConstruirObservacionTecnica(entrada.Valor, parametro.MinReferencial, parametro.MaxReferencial);

        if (existente is null)
        {
            existente = new ResultadoParametro
            {
                EnsayoRealizadoId = entrada.EnsayoRealizadoId,
                ParametroEnsayoId = entrada.ParametroEnsayoId,
                Valor = entrada.Valor,
                Observacion = entrada.Observacion,
                CumpleRango = cumpleRango,
                ObservacionTecnica = observacionTecnica
            };

            db.ResultadosParametro.Add(existente);
        }
        else
        {
            existente.Valor = entrada.Valor;
            existente.Observacion = entrada.Observacion;
            existente.CumpleRango = cumpleRango;
            existente.ObservacionTecnica = observacionTecnica;
        }

        await db.SaveChangesAsync();

        return new ResultadoOperacion<ResultadoParametro>
        {
            Exito = true,
            Mensaje = "Resultado registrado correctamente.",
            Datos = existente
        };
    }

    public async Task<ResultadoValidacionEnsayo> ValidarEnsayoAsync(int ensayoId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var ensayo = await db.EnsayosRealizados
            .Include(x => x.Muestra)
            .Include(x => x.TipoEnsayo)
            .FirstOrDefaultAsync(x => x.Id == ensayoId);

        if (ensayo is null)
        {
            return new ResultadoValidacionEnsayo
            {
                EnsayoId = ensayoId,
                EstadoEnsayo = "Incompleto",
                EstadoMuestra = "En análisis",
                Mensajes =
                {
                    new MensajeValidacion { Tipo = "Error", Texto = "El ensayo no existe." }
                }
            };
        }

        var parametros = await db.ParametrosEnsayo
            .Where(x => x.TipoEnsayoId == ensayo.TipoEnsayoId)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        var resultados = await db.ResultadosParametro
            .Include(x => x.ParametroEnsayo)
            .Where(x => x.EnsayoRealizadoId == ensayoId)
            .ToListAsync();

        var mensajes = new List<MensajeValidacion>();

        foreach (var parametro in parametros.Where(p => p.Requerido && !p.EsCalculado))
        {
            var existe = resultados.Any(r => r.ParametroEnsayoId == parametro.Id);
            if (!existe)
            {
                mensajes.Add(new MensajeValidacion
                {
                    Tipo = "Error",
                    Texto = $"Falta el parámetro obligatorio: {parametro.Nombre}."
                });
            }
        }

        foreach (var resultado in resultados)
        {
            if (resultado.CumpleRango == false)
            {
                mensajes.Add(new MensajeValidacion
                {
                    Tipo = "Advertencia",
                    Texto = $"Resultado fuera de rango: {resultado.ParametroEnsayo?.Nombre ?? "Parámetro"}."
                });
            }
        }

        var resumen = await ObtenerResumenMuestraInternoAsync(db, ensayo.MuestraId);

        if (resumen is not null &&
            resumen.LimiteLiquido.HasValue &&
            resumen.LimitePlastico.HasValue &&
            resumen.LimitePlastico > resumen.LimiteLiquido)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Error",
                Texto = "Inconsistencia técnica: LP es mayor que LL."
            });
        }

        if (resumen is not null &&
            resumen.IndicePlasticidad.HasValue &&
            resumen.IndicePlasticidad < 0)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Error",
                Texto = "Inconsistencia técnica: el índice de plasticidad es negativo."
            });
        }

        var faltantes = mensajes.Any(x => x.Tipo == "Error" && x.Texto.StartsWith("Falta el parámetro obligatorio"));
        var errores = mensajes.Any(x => x.Tipo == "Error" && !x.Texto.StartsWith("Falta el parámetro obligatorio"));
        var advertencias = mensajes.Any(x => x.Tipo == "Advertencia");

        var estadoEnsayo = "Validado";
        var estadoMuestra = "Validada";

        if (faltantes)
        {
            estadoEnsayo = "Incompleto";
            estadoMuestra = "Con resultados parciales";
        }
        else if (errores || advertencias)
        {
            estadoEnsayo = "Observado";
            estadoMuestra = "Observada";
        }

        ensayo.Estado = estadoEnsayo;

        if (estadoEnsayo == "Incompleto")
            ensayo.FechaValidacion = null;
        else
            ensayo.FechaValidacion = DateTime.Today;

        if (ensayo.Muestra is not null)
        {
            ensayo.Muestra.EstadoMuestra = estadoMuestra;
        }

        await db.SaveChangesAsync();

        return new ResultadoValidacionEnsayo
        {
            EnsayoId = ensayoId,
            EstadoEnsayo = estadoEnsayo,
            EstadoMuestra = estadoMuestra,
            Mensajes = mensajes
        };
    }

    public async Task<ResultadoResumenMuestra?> ObtenerResumenMuestraAsync(int muestraId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await ObtenerResumenMuestraInternoAsync(db, muestraId);
    }

    private async Task<ResultadoResumenMuestra?> ObtenerResumenMuestraInternoAsync(AppDbContext db, int muestraId)
    {
        var muestra = await db.Muestras
            .Include(x => x.PuntoMuestreo)
            .FirstOrDefaultAsync(x => x.Id == muestraId);

        if (muestra is null)
            return null;

        var resultados = await db.ResultadosParametro
            .Include(x => x.ParametroEnsayo)
            .Include(x => x.EnsayoRealizado)
                .ThenInclude(e => e!.TipoEnsayo)
            .Where(x => x.EnsayoRealizado!.MuestraId == muestraId)
            .ToListAsync();

        decimal? hum = BuscarValorPorCodigoEnsayo(resultados, "HUM");
        decimal? ll = BuscarValorPorCodigoEnsayo(resultados, "LL");
        decimal? lp = BuscarValorPorCodigoEnsayo(resultados, "LP");
        decimal? ip = null;

        if (ll.HasValue && lp.HasValue)
            ip = ll.Value - lp.Value;

        var mensajes = new List<MensajeValidacion>();

        if (!hum.HasValue)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Advertencia",
                Texto = "No se encontró resultado de Humedad Natural."
            });
        }

        if (!ll.HasValue)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Advertencia",
                Texto = "No se encontró resultado de Límite Líquido."
            });
        }

        if (!lp.HasValue)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Advertencia",
                Texto = "No se encontró resultado de Límite Plástico."
            });
        }

        if (ll.HasValue && lp.HasValue && lp > ll)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Error",
                Texto = "LP es mayor que LL."
            });
        }

        if (ip.HasValue && ip < 0)
        {
            mensajes.Add(new MensajeValidacion
            {
                Tipo = "Error",
                Texto = "El índice de plasticidad es negativo."
            });
        }

        var detalle = resultados
            .OrderBy(x => x.ParametroEnsayo!.Nombre)
            .Select(x => new ResultadoParametroResumen
            {
                Parametro = x.ParametroEnsayo?.Nombre ?? string.Empty,
                Valor = x.Valor,
                MinReferencial = x.ParametroEnsayo?.MinReferencial,
                MaxReferencial = x.ParametroEnsayo?.MaxReferencial,
                CumpleRango = x.CumpleRango,
                ObservacionTecnica = x.ObservacionTecnica
            })
            .ToList();

        return new ResultadoResumenMuestra
        {
            MuestraId = muestra.Id,
            CodigoMuestra = muestra.CodigoMuestra,
            EstadoMuestra = muestra.EstadoMuestra,
            HumedadNatural = hum,
            LimiteLiquido = ll,
            LimitePlastico = lp,
            IndicePlasticidad = ip,
            Mensajes = mensajes,
            Resultados = detalle
        };
    }

    private static decimal? BuscarValorPorCodigoEnsayo(List<ResultadoParametro> resultados, string codigoEnsayo)
    {
        var item = resultados.FirstOrDefault(x =>
            x.EnsayoRealizado?.TipoEnsayo?.Codigo != null &&
            x.EnsayoRealizado.TipoEnsayo.Codigo.Equals(codigoEnsayo, StringComparison.OrdinalIgnoreCase));

        return item?.Valor;
    }

    private static bool? CalcularCumpleRango(decimal valor, decimal? min, decimal? max)
    {
        if (!min.HasValue && !max.HasValue)
            return null;

        if (min.HasValue && max.HasValue && min > max)
            return null;

        if (min.HasValue && valor < min.Value)
            return false;

        if (max.HasValue && valor > max.Value)
            return false;

        return true;
    }

    private static string ConstruirObservacionTecnica(decimal valor, decimal? min, decimal? max)
    {
        if (!min.HasValue && !max.HasValue)
            return "El parámetro no tiene rango referencial configurado.";

        if (min.HasValue && max.HasValue && min > max.Value)
            return "El rango referencial del parámetro está mal configurado.";

        if (min.HasValue && valor < min.Value)
            return $"Valor fuera del rango referencial ({min} - {max}).";

        if (max.HasValue && valor > max.Value)
            return $"Valor fuera del rango referencial ({min} - {max}).";

        return "Valor dentro del rango referencial.";
    }
}