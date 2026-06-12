using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("inventory_movements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConsumableId).HasColumnName("consumable_id");
        builder.Property(m => m.ToolId).HasColumnName("tool_id");

        builder.Property(m => m.MovementType)
            .HasColumnName("movement_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(m => m.Quantity).HasColumnName("quantity");

        builder.Property(m => m.Reference)
            .HasColumnName("reference")
            .HasMaxLength(100);

        builder.Property(m => m.ReferenceId).HasColumnName("reference_id");

        builder.Property(m => m.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAt).HasColumnName("created_at");

        builder.HasOne(m => m.Consumable)
            .WithMany()
            .HasForeignKey(m => m.ConsumableId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Tool)
            .WithMany()
            .HasForeignKey(m => m.ToolId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
