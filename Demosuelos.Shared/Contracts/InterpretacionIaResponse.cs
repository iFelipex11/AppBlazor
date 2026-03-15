namespace Demosuelos.Shared.Contracts;

public class InterpretacionIaResponse
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Interpretacion { get; set; }
}