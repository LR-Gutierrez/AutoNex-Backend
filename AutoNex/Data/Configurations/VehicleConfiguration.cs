using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ClientId)
            .HasColumnName("client_id");

        builder.Property(v => v.Brand)
            .HasColumnName("brand")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Model)
            .HasColumnName("model")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Year)
            .HasColumnName("year");

        builder.Property(v => v.LicensePlate)
            .HasColumnName("license_plate")
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(v => v.LicensePlate).IsUnique();

        builder.Property(v => v.VIN)
            .HasColumnName("vin")
            .HasMaxLength(17);

        builder.Property(v => v.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(v => v.Client)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
