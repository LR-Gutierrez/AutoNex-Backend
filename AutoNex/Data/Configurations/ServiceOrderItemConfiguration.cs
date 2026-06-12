using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ServiceOrderItemConfiguration : IEntityTypeConfiguration<ServiceOrderItem>
{
    public void Configure(EntityTypeBuilder<ServiceOrderItem> builder)
    {
        builder.ToTable("service_order_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ServiceOrderId).HasColumnName("service_order_id");
        builder.Property(i => i.ServiceId).HasColumnName("service_id");
        builder.Property(i => i.ConsumableId).HasColumnName("consumable_id");
        builder.Property(i => i.ServiceVariantId).HasColumnName("service_variant_id");

        builder.Property(i => i.Quantity).HasColumnName("quantity");

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2);

        builder.Property(i => i.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(i => i.CreatedAt).HasColumnName("created_at");

        builder.HasOne(i => i.ServiceOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasOne(i => i.Service)
            .WithMany()
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(i => i.Consumable)
            .WithMany()
            .HasForeignKey(i => i.ConsumableId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ServiceVariant)
            .WithMany()
            .HasForeignKey(i => i.ServiceVariantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
