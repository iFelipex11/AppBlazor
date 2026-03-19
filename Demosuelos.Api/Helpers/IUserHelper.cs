using Demosuelos.Api.Entities;
using Demosuelos.Shared.DTOs;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Demosuelos.Api.Helpers;

public interface IUserHelper
{
    Task<User?> GetUserAsync(string email);

    Task<IdentityResult> AddUserAsync(User user, string password);

    Task CheckRoleAsync(UserType userType);

    Task AddUserToRoleAsync(User user, UserType userType);

    Task<bool> IsUserInRoleAsync(User user, UserType userType);

    Task<SignInResult> LoginAsync(LoginDTO model);

    Task LogoutAsync();

    Task<string> GenerateEmailConfirmationTokenAsync(User user);

    Task<IdentityResult> ConfirmEmailAsync(User user, string token);

    Task<string> GeneratePasswordResetTokenAsync(User user);

    Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);
}