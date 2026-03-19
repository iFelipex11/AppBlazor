using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Shared.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "Debes ingresar un correo válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}