using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class Muestra : AuditableEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un punto de muestreo.")]
    public int PuntoMuestreoId { get; set; }

    [Required]
    [MaxLength(50)]
    public string CodigoMuestra { get; set; } = string.Empty;

    public DateTime FechaRecepcion { get; set; } = DateTime.Today;

    public DateTime FechaMuestreo { get; set; } = DateTime.Today;

    public decimal? ProfundidadInicial { get; set; }

    public decimal? ProfundidadFinal { get; set; }

    [MaxLength(50)]
    public string? TipoMuestra { get; set; }

    [Required]
    [MaxLength(50)]
    public string EstadoMuestra { get; set; } = "Registrada";

    [MaxLength(300)]
    public string? Observaciones { get; set; }

    public PuntoMuestreo? PuntoMuestreo { get; set; }
}
