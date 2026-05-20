using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<RegistroAqua> RegistrosAgua { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<ContaAgua> ContasAgua { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<Usuario>()
            .HasMany(u => u.RegistrosAgua)
            .WithOne(r => r.Usuario)
            .HasForeignKey(r => r.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Usuario>()
            .HasMany(u => u.Notificacoes)
            .WithOne(n => n.Usuario)
            .HasForeignKey(n => n.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Usuario>()
            .HasMany(u => u.ContasAgua)
            .WithOne(c => c.Usuario)
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
