using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demosuelos.Models;

public class EnsayoRealizado : AuditableEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una muestra.")]
    public int MuestraId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un tipo de ensayo.")]
    public int TipoEnsayoId { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.Today;

    public DateTime FechaEjecucion { get; set; } = DateTime.Today;

    public DateTime? FechaValidacion { get; set; }

    [MaxLength(100)]
    public string? Responsable { get; set; }

    [Required]
    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente";

    public Muestra? Muestra { get; set; }
    public TipoEnsayo? TipoEnsayo { get; set; }

    [NotMapped]
    public DateTime FechaEnsayo
    {
        get => FechaEjecucion;
        set => FechaEjecucion = value;
    }
}
