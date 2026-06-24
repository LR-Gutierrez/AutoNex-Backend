using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.ToTable("message_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Key)
            .HasColumnName("key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Template)
            .HasColumnName("template")
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(t => t.Key).IsUnique();
    }
}
