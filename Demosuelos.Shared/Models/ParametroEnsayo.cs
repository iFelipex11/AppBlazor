using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Models;

public class ParametroEnsayo : AuditableEntity
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un tipo de ensayo.")]
    public int TipoEnsayoId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Unidad { get; set; }

    public bool Requerido { get; set; } = true;

    public bool EsCalculado { get; set; } = false;

    public decimal? MinReferencial { get; set; }

    public decimal? MaxReferencial { get; set; }

    public TipoEnsayo? TipoEnsayo { get; set; }
}
