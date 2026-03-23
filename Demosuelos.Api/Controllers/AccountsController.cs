using Demosuelos.Api.Entities;
using Demosuelos.Api.Data;
using Demosuelos.Api.Helpers;
using Demosuelos.Shared.DTOs;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Demosuelos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IUserHelper _userHelper;
    private readonly IConfiguration _configuration;
    private readonly AppDbSeeder _dbSeeder;

    public AccountsController(
        IUserHelper userHelper,
        IConfiguration configuration,
        AppDbSeeder dbSeeder)
    {
        _userHelper = userHelper;
        _configuration = configuration;
        _dbSeeder = dbSeeder;
    }

    [HttpPost("createuser")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTO>> CreateUser([FromBody] UserDTO model)
    {
        var user = await _userHelper.GetUserAsync(model.Email);
        if (user is not null)
        {
            return BadRequest("Ya existe un usuario registrado con ese correo.");
        }

        user = new User
        {
            Document = model.Document,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Address = model.Address,
            UserType = model.UserType,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await _userHelper.AddUserAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        await _userHelper.CheckRoleAsync(model.UserType);
        await _userHelper.AddUserToRoleAsync(user, model.UserType);
        return Ok(BuildToken(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTO>> Login([FromBody] LoginDTO model)
    {
        var result = await _userHelper.LoginAsync(model);
        if (!result.Succeeded)
        {
            return BadRequest("Email o contrasena incorrectos.");
        }

        var user = await _userHelper.GetUserAsync(model.Email);
        if (user is null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return BadRequest("El usuario se encuentra inactivo.");
        }

        return Ok(BuildToken(user));
    }

    [HttpPost("forgotpassword")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
    {
        var user = await _userHelper.GetUserAsync(model.Email);
        if (user is null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
        var result = await _userHelper.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        return Ok(new
        {
            message = "La contrasena fue actualizada correctamente."
        });
    }

    [HttpPost("bootstrap-admin")]
    [AllowAnonymous]
    public async Task<ActionResult> BootstrapAdmin()
    {
        var result = await _dbSeeder.BootstrapAdminAsync();
        if (!result.Created)
        {
            return BadRequest(result.Message);
        }

        return Ok(new
        {
            message = result.Message
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserAdminDTO>> Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var user = await _userHelper.GetUserAsync(email);
        if (user is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        return Ok(ToAdminDto(user));
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserListDTO>>> GetUsers()
    {
        var users = await _userHelper.GetUsersAsync();
        return Ok(users.Select(ToListDto).ToList());
    }

    [HttpGet("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserAdminDTO>> GetUser(string id)
    {
        var user = await _userHelper.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        return Ok(ToAdminDto(user));
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateUser(string id, [FromBody] UserAdminDTO model)
    {
        if (id != model.Id)
        {
            return BadRequest("El id de la ruta no coincide con el usuario.");
        }

        var user = await _userHelper.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var otherUser = await _userHelper.GetUserAsync(model.Email);
        if (otherUser is not null && otherUser.Id != user.Id)
        {
            return BadRequest("Ya existe un usuario con ese correo.");
        }

        var currentRoles = await _userHelper.GetRolesAsync(user);
        foreach (var role in currentRoles)
        {
            if (Enum.TryParse<UserType>(role, out var roleType))
            {
                await _userHelper.RemoveUserFromRoleAsync(user, roleType);
            }
        }

        await _userHelper.CheckRoleAsync(model.UserType);

        user.Document = model.Document;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.UserType = model.UserType;
        user.LockoutEnabled = true;
        user.LockoutEnd = model.IsActive ? null : DateTimeOffset.MaxValue;

        var result = await _userHelper.UpdateUserAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        await _userHelper.AddUserToRoleAsync(user, model.UserType);
        return NoContent();
    }

    private UserListDTO ToListDto(User user)
    {
        return new UserListDTO
        {
            Id = user.Id,
            Document = user.Document,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            UserType = user.UserType,
            IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow
        };
    }

    private UserAdminDTO ToAdminDto(User user)
    {
        return new UserAdminDTO
        {
            Id = user.Id,
            Document = user.Document,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            Email = user.Email ?? string.Empty,
            UserType = user.UserType,
            IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow
        };
    }

    private TokenDTO BuildToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.UserType.ToString()),
            new("document", user.Document),
            new("fullname", user.FullName),
            new("usertype", user.UserType.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return new TokenDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration
        };
    }
}
