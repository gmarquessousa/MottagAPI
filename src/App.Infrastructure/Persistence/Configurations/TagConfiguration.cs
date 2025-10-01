using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags", "dbo", tbl =>
        {
            tbl.HasCheckConstraint("CK_Tag_BateriaPct", "[BateriaPct] BETWEEN 0 AND 100");
        });
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Serial).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Serial).IsUnique();
        builder.HasIndex(t => t.MotoId).IsUnique().HasFilter("[MotoId] IS NOT NULL");
    // CreatedAt removido; índice excluído.
        builder.Property(t => t.BateriaPct).HasDefaultValue(0);
    }
}
