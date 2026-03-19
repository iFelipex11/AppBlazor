using System.ComponentModel.DataAnnotations;

namespace Demosuelos.Shared.Enums;

public enum UserType
{
    [Display(Name = "Administrador")]
    Admin,

    [Display(Name = "Usuario")]
    User
}