using Demosuelos.Api.Entities;
using Demosuelos.Shared.DTOs;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Demosuelos.Api.Helpers;

public class UserHelper : IUserHelper
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserHelper(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<User?> GetUserAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IdentityResult> AddUserAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task CheckRoleAsync(UserType userType)
    {
        var roleName = userType.ToString();
        var exists = await _roleManager.RoleExistsAsync(roleName);

        if (!exists)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    public async Task AddUserToRoleAsync(User user, UserType userType)
    {
        await _userManager.AddToRoleAsync(user, userType.ToString());
    }

    public async Task<bool> IsUserInRoleAsync(User user, UserType userType)
    {
        return await _userManager.IsInRoleAsync(user, userType.ToString());
    }

    public async Task<SignInResult> LoginAsync(LoginDTO model)
    {
        return await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
    {
        return await _userManager.ResetPasswordAsync(user, token, password);
    }
}