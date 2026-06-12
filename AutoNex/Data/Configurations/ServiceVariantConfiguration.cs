using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ServiceVariantConfiguration : IEntityTypeConfiguration<ServiceVariant>
{
    public void Configure(EntityTypeBuilder<ServiceVariant> builder)
    {
        builder.ToTable("service_variants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.ServiceId)
            .HasColumnName("service_id");

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(v => v.MinKmInterval)
            .HasColumnName("min_km_interval");

        builder.Property(v => v.MaxKmInterval)
            .HasColumnName("max_km_interval");

        builder.Property(v => v.RecommendedMonths)
            .HasColumnName("recommended_months");

        builder.Property(v => v.IsActive)
            .HasColumnName("is_active");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(v => v.Service)
            .WithMany(s => s.Variants)
            .HasForeignKey(v => v.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(v => v.IsActive);
    }
}
