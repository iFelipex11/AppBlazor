using Demosuelos.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Shared.DTOs;

public class UserDTO
{
    [Display(Name = "Documento")]
    [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Document { get; set; } = null!;

    [Display(Name = "Nombres")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Apellidos")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Dirección")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Address { get; set; } = null!;

    [Display(Name = "Tipo de usuario")]
    public UserType UserType { get; set; } = UserType.User;

    [Display(Name = "Correo")]
    [EmailAddress(ErrorMessage = "Debes ingresar un correo válido.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Email { get; set; } = null!;

    [Display(Name = "Contraseña")]
    [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Password { get; set; } = null!;

    [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación no son iguales.")]
    [Display(Name = "Confirmación de contraseña")]
    [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string PasswordConfirm { get; set; } = null!;
}
