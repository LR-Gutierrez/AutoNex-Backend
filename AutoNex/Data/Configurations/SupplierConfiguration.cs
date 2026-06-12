using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(200);

        builder.Property(s => s.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(s => s.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

        builder.Property(s => s.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
