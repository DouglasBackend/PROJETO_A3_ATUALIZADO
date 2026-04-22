using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

/// <summary>
/// Contexto do banco de dados com suporte multitenant
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IProvedorTenant? _provedorTenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, IProvedorTenant? provedorTenant = null)
        : base(options)
    {
        _provedorTenant = provedorTenant;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<RegistroAqua> RegistrosAgua { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Índices únicos
        builder.Entity<Tenant>()
            .HasIndex(t => t.Identificador)
            .IsUnique();

        builder.Entity<Usuario>()
            .HasIndex(u => new { u.IdTenant, u.Email })
            .IsUnique();

        // Relacionamentos
        builder.Entity<Usuario>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Usuarios)
            .HasForeignKey(u => u.IdTenant)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RegistroAqua>()
            .HasOne(r => r.Usuario)
            .WithMany(u => u.RegistrosAgua)
            .HasForeignKey(r => r.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RegistroAqua>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.IdTenant)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notificacao>()
            .HasOne(n => n.Usuario)
            .WithMany(u => u.Notificacoes)
            .HasForeignKey(n => n.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notificacao>()
            .HasOne(n => n.Tenant)
            .WithMany()
            .HasForeignKey(n => n.IdTenant)
            .OnDelete(DeleteBehavior.Cascade);

        // Configurações de tabelas
        builder.Entity<Tenant>()
            .ToTable("tenants");

        builder.Entity<Usuario>()
            .ToTable("usuarios");

        builder.Entity<RegistroAqua>()
            .ToTable("registros_agua");

        builder.Entity<Notificacao>()
            .ToTable("notificacoes");
    }

    public override int SaveChanges()
    {
        AdicionarIdTenantAoRegistrosAlterados();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AdicionarIdTenantAoRegistrosAlterados();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AdicionarIdTenantAoRegistrosAlterados()
    {
        if (_provedorTenant == null)
            return;

        var tenantId = _provedorTenant.ObterIdTenantAtual();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IEntidadeTenant entidadeTenant)
            {
                if (entry.State == EntityState.Added)
                {
                    entidadeTenant.IdTenant = tenantId;
                }
            }
        }
    }
}

/// <summary>
/// Interface para marcar entidades que pertencem a um tenant
/// </summary>
public interface IEntidadeTenant
{
    int IdTenant { get; set; }
}

/// <summary>
/// Interface para fornecer o ID do tenant atual
/// </summary>
public interface IProvedorTenant
{
    int ObterIdTenantAtual();
    void DefinirIdTenantAtual(int tenantId);
}
