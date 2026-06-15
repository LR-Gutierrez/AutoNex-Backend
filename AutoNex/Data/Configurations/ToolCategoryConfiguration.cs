using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ToolCategoryConfiguration : IEntityTypeConfiguration<ToolCategory>
{
    public void Configure(EntityTypeBuilder<ToolCategory> builder)
    {
        builder.ToTable("tool_categories");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(tc => tc.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(tc => tc.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(tc => !tc.IsDeleted);
    }
}
