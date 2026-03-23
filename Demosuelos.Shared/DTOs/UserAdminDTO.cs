using Demosuelos.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Shared.DTOs;

public class UserAdminDTO
{
    public string Id { get; set; } = null!;

    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Document { get; set; } = null!;

    [Display(Name = "Nombres")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Apellidos")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Direccion")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
    public string Email { get; set; } = null!;

    public UserType UserType { get; set; }

    public bool IsActive { get; set; }
}
