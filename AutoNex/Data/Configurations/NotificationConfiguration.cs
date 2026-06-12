using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.ClientId).HasColumnName("client_id");
        builder.Property(n => n.VehicleId).HasColumnName("vehicle_id");

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Recipient)
            .HasColumnName("recipient")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.SentAt).HasColumnName("sent_at");

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(n => n.CreatedAt).HasColumnName("created_at");

        builder.HasQueryFilter(n => !n.IsDeleted);

        builder.HasOne(n => n.Client)
            .WithMany()
            .HasForeignKey(n => n.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Vehicle)
            .WithMany()
            .HasForeignKey(n => n.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
