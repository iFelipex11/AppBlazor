using Demosuelos.Api.Data;
using Demosuelos.Api.Entities;
using Demosuelos.Api.Helpers;
using Demosuelos.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontro la cadena de conexion 'DefaultConnection'. Revisa appsettings.json o variables de entorno.");
}

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);
builder.Services.Configure<SeedUserOptions>(builder.Configuration.GetSection("SeedUser"));
builder.Services.Configure<DatabaseStartupOptions>(builder.Configuration.GetSection("DatabaseStartup"));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<AppDbSeeder>();
builder.Services.AddHttpClient<GeminiInterpretacionService>();
builder.Services.AddScoped<EnsayoReglasService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    context.Fail("Token sin email.");
                    return;
                }

                var userHelper = context.HttpContext.RequestServices.GetRequiredService<IUserHelper>();
                var user = await userHelper.GetUserAsync(email);
                if (user is null)
                {
                    context.Fail("Usuario no encontrado.");
                    return;
                }

                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    context.Fail("Usuario inactivo.");
                }
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddCors(options =>
{
    options.AddPolicy("blazor-client", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Demosuelos API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seeder temporalmente deshabilitado para validar el arranque base de la API
// sin depender del estado previo de Identity en la base existente.
// using (var scope = app.Services.CreateScope())
// {
//     var seeder = scope.ServiceProvider.GetRequiredService<AppDbSeeder>();
//     await seeder.SeedAsync();
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("blazor-client");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
