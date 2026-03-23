using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class Proyecto : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Cliente { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Ubicacion { get; set; }

    [Required]
    [MaxLength(50)]
    public string Estado { get; set; } = "Activo";
}
