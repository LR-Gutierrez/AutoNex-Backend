using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class WhatsAppMessageLogConfiguration : IEntityTypeConfiguration<WhatsAppMessageLog>
{
    public void Configure(EntityTypeBuilder<WhatsAppMessageLog> builder)
    {
        builder.ToTable("whatsapp_message_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        builder.Property(x => x.SentBy)
            .HasColumnName("sent_by")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
    }
}
