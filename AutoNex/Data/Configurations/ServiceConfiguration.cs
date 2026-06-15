using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.DefaultPrice)
            .HasColumnName("default_price")
            .HasPrecision(18, 2);

        builder.Property(s => s.MinKmInterval)
            .HasColumnName("min_km_interval");

        builder.Property(s => s.MaxKmInterval)
            .HasColumnName("max_km_interval");

        builder.Property(s => s.RecommendedMonths)
            .HasColumnName("recommended_months");

        builder.Property(s => s.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
