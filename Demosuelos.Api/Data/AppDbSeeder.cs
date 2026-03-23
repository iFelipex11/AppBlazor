using Demosuelos.Api.Entities;
using Demosuelos.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demosuelos.Api.Data;

public class AppDbSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SeedUserOptions _seedUser;
    private readonly DatabaseStartupOptions _startupOptions;
    private readonly ILogger<AppDbSeeder> _logger;

    public AppDbSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<SeedUserOptions> seedUser,
        IOptions<DatabaseStartupOptions> startupOptions,
        ILogger<AppDbSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _seedUser = seedUser.Value;
        _startupOptions = startupOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_startupOptions.ApplyMigrationsOnStartup)
        {
            await _context.Database.MigrateAsync();
        }
        else
        {
            _logger.LogInformation("Las migraciones automaticas al iniciar estan deshabilitadas.");
        }

        if (!await IdentityTablesExistAsync())
        {
            _logger.LogWarning("No se encontraron las tablas de Identity. Se omite el seed de seguridad.");
            return;
        }

        await EnsureSecuritySeedAsync();
    }

    public async Task<(bool Created, string Message)> BootstrapAdminAsync()
    {
        if (!await IdentityTablesExistAsync())
        {
            return (false, "No existen las tablas de Identity en la base de datos.");
        }

        var hasUsers = await _userManager.Users.AnyAsync();
        if (hasUsers)
        {
            return (false, "Ya existen usuarios registrados. El bootstrap inicial ya no esta disponible.");
        }

        await EnsureSecuritySeedAsync();
        return (true, "Usuario administrador inicial creado correctamente.");
    }

    private async Task EnsureSecuritySeedAsync()
    {
        foreach (var role in Enum.GetNames<UserType>())
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await _userManager.FindByEmailAsync(_seedUser.Email);
        if (admin is not null)
        {
            if (!await _userManager.IsInRoleAsync(admin, UserType.Admin.ToString()))
            {
                await _userManager.AddToRoleAsync(admin, UserType.Admin.ToString());
            }

            admin.UserType = UserType.Admin;
            admin.Document = _seedUser.Document;
            admin.FirstName = _seedUser.FirstName;
            admin.LastName = _seedUser.LastName;
            admin.Address = _seedUser.Address;
            admin.UserName = _seedUser.Email;
            admin.Email = _seedUser.Email;
            admin.LockoutEnabled = true;
            admin.LockoutEnd = null;
            await _userManager.UpdateAsync(admin);
            return;
        }

        admin = new User
        {
            UserName = _seedUser.Email,
            Email = _seedUser.Email,
            Document = _seedUser.Document,
            FirstName = _seedUser.FirstName,
            LastName = _seedUser.LastName,
            Address = _seedUser.Address,
            UserType = UserType.Admin,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(admin, _seedUser.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"No fue posible crear el usuario administrador inicial: {errors}");
        }

        await _userManager.AddToRoleAsync(admin, UserType.Admin.ToString());
    }

    private async Task<bool> IdentityTablesExistAsync()
    {
        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME IN ('AspNetUsers', 'AspNetRoles')
            """;

        var result = await command.ExecuteScalarAsync();
        var count = Convert.ToInt32(result);
        return count >= 2;
    }
}
