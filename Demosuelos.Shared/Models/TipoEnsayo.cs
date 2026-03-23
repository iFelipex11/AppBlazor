using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class TipoEnsayo : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}
