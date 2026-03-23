using Demosuelos.Api.Entities;
using Demosuelos.Shared.DTOs;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _userManager.Users
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();
    }

    public async Task<IdentityResult> AddUserAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
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

    public async Task RemoveUserFromRoleAsync(User user, UserType userType)
    {
        await _userManager.RemoveFromRoleAsync(user, userType.ToString());
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

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}
