using Demosuelos.Models;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Api.Entities;

public class User : IdentityUser, IAuditableEntity
{
    [Display(Name = "Documento")]
    [MaxLength(20, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Document { get; set; } = null!;

    [Display(Name = "Nombres")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string FirstName { get; set; } = null!;

    [Display(Name = "Apellidos")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Direccion")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Address { get; set; } = null!;

    [Display(Name = "Tipo de usuario")]
    public UserType UserType { get; set; }

    [Display(Name = "Usuario")]
    public string FullName => $"{FirstName} {LastName}";

    public DateTime FechaCreacion { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? ActualizadoPor { get; set; }
}
