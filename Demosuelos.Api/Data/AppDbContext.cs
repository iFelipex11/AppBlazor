using Demosuelos.Api.Entities;
using Demosuelos.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Claims;

namespace Demosuelos.Api.Data;

public class AppDbContext : IdentityDbContext<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<PuntoMuestreo> PuntosMuestreo => Set<PuntoMuestreo>();
    public DbSet<Muestra> Muestras => Set<Muestra>();
    public DbSet<TipoEnsayo> TiposEnsayo => Set<TipoEnsayo>();
    public DbSet<ParametroEnsayo> ParametrosEnsayo => Set<ParametroEnsayo>();
    public DbSet<EnsayoRealizado> EnsayosRealizados => Set<EnsayoRealizado>();
    public DbSet<ResultadoParametro> ResultadosParametro => Set<ResultadoParametro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Document).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(200).IsRequired();

            ConfigureAuditable(entity);
            entity.HasIndex(x => x.Document).IsUnique();
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Cliente).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Ubicacion).HasMaxLength(200);
            entity.Property(x => x.Estado).HasMaxLength(50).IsRequired();
            ConfigureAuditable(entity);
        });

        modelBuilder.Entity<PuntoMuestreo>(entity =>
        {
            entity.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Sector).HasMaxLength(100);
            entity.Property(x => x.Descripcion).HasMaxLength(200);
            entity.Property(x => x.CoordenadaX).HasMaxLength(100);
            entity.Property(x => x.CoordenadaY).HasMaxLength(100);

            ConfigureAuditable(entity);
            entity.HasIndex(x => new { x.ProyectoId, x.Codigo }).IsUnique();

            entity.HasOne(x => x.Proyecto)
                .WithMany()
                .HasForeignKey(x => x.ProyectoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Muestra>(entity =>
        {
            entity.Property(x => x.CodigoMuestra).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FechaRecepcion)
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(GETDATE() AS date)");
            entity.Property(x => x.FechaMuestreo)
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(GETDATE() AS date)");
            entity.Property(x => x.ProfundidadInicial).HasPrecision(18, 2);
            entity.Property(x => x.ProfundidadFinal).HasPrecision(18, 2);
            entity.Property(x => x.TipoMuestra).HasMaxLength(50);
            entity.Property(x => x.EstadoMuestra).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Observaciones).HasMaxLength(300);

            ConfigureAuditable(entity);
            entity.HasIndex(x => x.CodigoMuestra).IsUnique();

            entity.HasOne(x => x.PuntoMuestreo)
                .WithMany()
                .HasForeignKey(x => x.PuntoMuestreoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TipoEnsayo>(entity =>
        {
            entity.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(300);

            ConfigureAuditable(entity);
            entity.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<ParametroEnsayo>(entity =>
        {
            entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Unidad).HasMaxLength(50);
            entity.Property(x => x.EsCalculado).HasDefaultValue(false);
            entity.Property(x => x.MinReferencial).HasPrecision(18, 4);
            entity.Property(x => x.MaxReferencial).HasPrecision(18, 4);

            ConfigureAuditable(entity);
            entity.HasIndex(x => new { x.TipoEnsayoId, x.Nombre }).IsUnique();

            entity.HasOne(x => x.TipoEnsayo)
                .WithMany()
                .HasForeignKey(x => x.TipoEnsayoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EnsayoRealizado>(entity =>
        {
            entity.Property(x => x.FechaAsignacion)
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(GETDATE() AS date)");
            entity.Property(x => x.FechaEjecucion)
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(GETDATE() AS date)");
            entity.Property(x => x.FechaValidacion)
                .HasColumnType("date");
            entity.Property(x => x.Responsable).HasMaxLength(100);
            entity.Property(x => x.Estado).HasMaxLength(50).IsRequired();

            ConfigureAuditable(entity);
            entity.HasIndex(x => new { x.MuestraId, x.TipoEnsayoId }).IsUnique();

            entity.HasOne(x => x.Muestra)
                .WithMany()
                .HasForeignKey(x => x.MuestraId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TipoEnsayo)
                .WithMany()
                .HasForeignKey(x => x.TipoEnsayoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResultadoParametro>(entity =>
        {
            entity.Property(x => x.Valor).HasPrecision(18, 4);
            entity.Property(x => x.Observacion).HasMaxLength(300);
            entity.Property(x => x.ObservacionTecnica).HasMaxLength(300);

            ConfigureAuditable(entity);
            entity.HasIndex(x => new { x.EnsayoRealizadoId, x.ParametroEnsayoId }).IsUnique();

            entity.HasOne(x => x.EnsayoRealizado)
                .WithMany()
                .HasForeignKey(x => x.EnsayoRealizadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParametroEnsayo)
                .WithMany()
                .HasForeignKey(x => x.ParametroEnsayoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = DateTime.UtcNow;
        var currentUser = GetCurrentUser();

        foreach (var entry in ChangeTracker.Entries().Where(IsAuditableEntry))
        {
            if (entry.Entity is not IAuditableEntity auditable)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                if (auditable.FechaCreacion == default)
                {
                    auditable.FechaCreacion = now;
                }

                auditable.CreadoPor ??= currentUser;
                auditable.FechaActualizacion = null;
                auditable.ActualizadoPor = null;
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.FechaCreacion)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreadoPor)).IsModified = false;
                auditable.FechaActualizacion = now;
                auditable.ActualizadoPor = currentUser;
            }
        }
    }

    private static bool IsAuditableEntry(EntityEntry entry)
    {
        return entry.Entity is IAuditableEntity &&
               (entry.State == EntityState.Added || entry.State == EntityState.Modified);
    }

    private string GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var email = user?.FindFirstValue(ClaimTypes.Email);
        var name = user?.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return "Sistema";
    }

    private static void ConfigureAuditable<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : class, IAuditableEntity
    {
        entity.Property(x => x.FechaCreacion)
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(x => x.CreadoPor)
            .HasMaxLength(256);

        entity.Property(x => x.ActualizadoPor)
            .HasMaxLength(256);
    }
}
