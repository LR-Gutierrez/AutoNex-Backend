using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        builder.ToTable("tools");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.ToolCategoryId)
            .HasColumnName("tool_category_id")
            .IsRequired();

        builder.HasOne(t => t.ToolCategory)
            .WithMany()
            .HasForeignKey(t => t.ToolCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Quantity)
            .HasColumnName("quantity");

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.PurchaseDate)
            .HasColumnName("purchase_date");

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
