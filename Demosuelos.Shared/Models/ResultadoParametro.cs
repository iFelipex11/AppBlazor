using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class ResultadoParametro : AuditableEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un ensayo realizado.")]
    public int EnsayoRealizadoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un parámetro.")]
    public int ParametroEnsayoId { get; set; }

    [Required]
    public decimal Valor { get; set; }

    [MaxLength(300)]
    public string? Observacion { get; set; }

    public bool? CumpleRango { get; set; }

    [MaxLength(300)]
    public string? ObservacionTecnica { get; set; }

    public EnsayoRealizado? EnsayoRealizado { get; set; }
    public ParametroEnsayo? ParametroEnsayo { get; set; }
}
