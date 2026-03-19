using Demosuelos.Api.Entities;
using Demosuelos.Api.Helpers;
using Demosuelos.Shared.DTOs;
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

    public AccountsController(IUserHelper userHelper, IConfiguration configuration)
    {
        _userHelper = userHelper;
        _configuration = configuration;
    }

    [HttpPost("createuser")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateUser(UserDTO model)
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
            Photo = model.Photo,
            UserType = model.UserType,
            Email = model.Email,
            UserName = model.Email
        };

        var result = await _userHelper.AddUserAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userHelper.CheckRoleAsync(model.UserType);
            await _userHelper.AddUserToRoleAsync(user, model.UserType);

            return Ok(new
            {
                message = "Usuario creado correctamente."
            });
        }

        return BadRequest(result.Errors.Select(x => x.Description));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenDTO>> Login(LoginDTO model)
    {
        var result = await _userHelper.LoginAsync(model);
        if (!result.Succeeded)
        {
            return BadRequest("Email o contraseña incorrectos.");
        }

        var user = await _userHelper.GetUserAsync(model.Email);
        if (user is null)
        {
            return BadRequest("Usuario no encontrado.");
        }

        return Ok(BuildToken(user));
    }

    private TokenDTO BuildToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("document", user.Document),
            new("fullname", user.FullName),
            new("usertype", user.UserType.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddDays(30);

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