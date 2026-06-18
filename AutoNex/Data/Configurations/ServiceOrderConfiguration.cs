using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.VehicleId).HasColumnName("vehicle_id");
        builder.Property(o => o.ClientId).HasColumnName("client_id");
        builder.Property(o => o.UserId).HasColumnName("user_id");

        builder.Property(o => o.CurrentKm).HasColumnName("current_km");
        builder.Property(o => o.EstimatedDailyKm).HasColumnName("estimated_daily_km");
        builder.Property(o => o.DaysPerWeek).HasColumnName("days_per_week");

        builder.Property(o => o.Date).HasColumnName("date");

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 2);

        builder.Property(o => o.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(o => o.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.OperationNumber)
            .HasColumnName("operation_number")
            .HasMaxLength(100);

        builder.Property(o => o.OperationDate)
            .HasColumnName("operation_date")
            .HasColumnType("date");

        builder.Property(o => o.IsDeleted).HasColumnName("is_deleted");
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(o => o.Vehicle)
            .WithMany()
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Client)
            .WithMany()
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
