using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class MileageAlertConfiguration : IEntityTypeConfiguration<MileageAlert>
{
    public void Configure(EntityTypeBuilder<MileageAlert> builder)
    {
        builder.ToTable("mileage_alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.VehicleId)
            .HasColumnName("vehicle_id");

        builder.Property(a => a.EstimatedWeeklyKm)
            .HasColumnName("estimated_weekly_km");

        builder.Property(a => a.NextAlertKm)
            .HasColumnName("next_alert_km");

        builder.Property(a => a.LastAlertDate)
            .HasColumnName("last_alert_date");

        builder.Property(a => a.NextAlertDate)
            .HasColumnName("next_alert_date");

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(a => a.Vehicle)
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
