using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
