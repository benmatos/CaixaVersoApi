using CaixaVersoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CaixaVersoApi.Data;

public class CaixaVersoDbContext : DbContext
{
    public CaixaVersoDbContext(DbContextOptions<CaixaVersoDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Nome).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.SenhaHash).IsRequired();
            entity.Property(u => u.Cargo).HasMaxLength(100);
        });
    }
}
