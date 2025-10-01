using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patio> Patios => Set<Patio>();
    public DbSet<Moto> Motos => Set<Moto>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configura delete behavior para evitar múltiplos caminhos de cascade no SQL Server
        modelBuilder.Entity<Moto>()
            .HasOne(m => m.Patio)
            .WithMany(p => p.Motos)
            .HasForeignKey(m => m.PatioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tag>()
            .HasOne(t => t.Moto)
            .WithOne(m => m.Tag)
            .HasForeignKey<Tag>(t => t.MotoId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<Moto>()
            .HasIndex(m => m.Placa)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Serial)
            .IsUnique();

    }
}
