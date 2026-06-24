using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class WorkshopInfoConfiguration : IEntityTypeConfiguration<WorkshopInfo>
{
    public void Configure(EntityTypeBuilder<WorkshopInfo> builder)
    {
        builder.ToTable("workshop_info");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Rif)
            .HasColumnName("rif")
            .HasMaxLength(20);

        builder.Property(w => w.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(w => w.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(w => w.MapsUrl)
            .HasColumnName("maps_url")
            .HasMaxLength(500);

        builder.Property(w => w.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(w => w.SecondaryPhone)
            .HasColumnName("secondary_phone")
            .HasMaxLength(50);

        builder.Property(w => w.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

        builder.Property(w => w.Website)
            .HasColumnName("website")
            .HasMaxLength(200);

        builder.Property(w => w.OpeningHours)
            .HasColumnName("opening_hours")
            .HasMaxLength(200);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
