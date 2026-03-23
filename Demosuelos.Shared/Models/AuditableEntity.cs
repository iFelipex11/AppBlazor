namespace Demosuelos.Models;

public interface IAuditableEntity
{
    DateTime FechaCreacion { get; set; }
    string? CreadoPor { get; set; }
    DateTime? FechaActualizacion { get; set; }
    string? ActualizadoPor { get; set; }
}

public abstract class AuditableEntity : IAuditableEntity
{
    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? ActualizadoPor { get; set; }
}
