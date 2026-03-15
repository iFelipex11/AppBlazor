using Demosuelos.Api.Data;
using Demosuelos.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<GeminiInterpretacionService>();
builder.Services.AddScoped<EnsayoReglasService>();

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

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("blazor-client");

app.MapControllers();

app.Run();