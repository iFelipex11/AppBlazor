using Demosuelos.Shared.Enums;

namespace Demosuelos.Shared.DTOs;

public class UserListDTO
{
    public string Id { get; set; } = null!;
    public string Document { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; }
    public bool IsActive { get; set; }
}
