using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class PuntoMuestreo : AuditableEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un proyecto.")]
    public int ProyectoId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Sector { get; set; }

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [MaxLength(100)]
    public string? CoordenadaX { get; set; }

    [MaxLength(100)]
    public string? CoordenadaY { get; set; }

    public Proyecto? Proyecto { get; set; }
}
