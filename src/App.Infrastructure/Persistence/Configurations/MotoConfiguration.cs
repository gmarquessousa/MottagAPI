using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class MotoConfiguration : IEntityTypeConfiguration<Moto>
{
    public void Configure(EntityTypeBuilder<Moto> builder)
    {
        builder.ToTable("Motos", "dbo", tbl =>
        {
            tbl.HasCheckConstraint("CK_Moto_Status", "[Status] IN (0,1,2,3)");
        });
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Placa).IsRequired().HasMaxLength(20);
        builder.HasIndex(m => m.Placa).IsUnique();
        builder.Property(m => m.Modelo).IsRequired().HasMaxLength(120);
        builder.HasIndex(m => new { m.PatioId, m.Status });
        builder.HasOne(m => m.Patio).WithMany(p => p.Motos).HasForeignKey(m => m.PatioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Tag).WithOne(t => t.Moto).HasForeignKey<Tag>(t => t.MotoId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    // Navegação de Movimentos removida na versão simplificada
    }
}
