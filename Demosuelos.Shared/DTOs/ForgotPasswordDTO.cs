using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Shared.DTOs;

public class ForgotPasswordDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
    public string Email { get; set; } = null!;

    [Display(Name = "Nueva contrasena")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
    public string NewPassword { get; set; } = null!;

    [Display(Name = "Confirmacion de contrasena")]
    [Compare(nameof(NewPassword), ErrorMessage = "La contrasena y la confirmacion no son iguales.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ConfirmPassword { get; set; } = null!;
}
