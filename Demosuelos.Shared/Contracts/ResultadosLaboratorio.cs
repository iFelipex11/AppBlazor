namespace Demosuelos.Shared.Contracts;

public class MensajeValidacion
{
    public string Tipo { get; set; } = string.Empty; // Error, Advertencia, Info
    public string Texto { get; set; } = string.Empty;
}

public class ResultadoOperacion<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Datos { get; set; }
    public List<MensajeValidacion> Mensajes { get; set; } = new();
}

public class ResultadoParametroResumen
{
    public string Parametro { get; set; } = string.Empty;
    public decimal? Valor { get; set; }
    public decimal? MinReferencial { get; set; }
    public decimal? MaxReferencial { get; set; }
    public bool? CumpleRango { get; set; }
    public string? ObservacionTecnica { get; set; }
}

public class ResultadoValidacionEnsayo
{
    public int EnsayoId { get; set; }
    public string EstadoEnsayo { get; set; } = "Incompleto";
    public string EstadoMuestra { get; set; } = "En análisis";
    public List<MensajeValidacion> Mensajes { get; set; } = new();
}

public class ResultadoResumenMuestra
{
    public int MuestraId { get; set; }
    public string CodigoMuestra { get; set; } = string.Empty;
    public string EstadoMuestra { get; set; } = string.Empty;

    public decimal? HumedadNatural { get; set; }
    public decimal? LimiteLiquido { get; set; }
    public decimal? LimitePlastico { get; set; }
    public decimal? IndicePlasticidad { get; set; }

    public List<MensajeValidacion> Mensajes { get; set; } = new();
    public List<ResultadoParametroResumen> Resultados { get; set; } = new();
}