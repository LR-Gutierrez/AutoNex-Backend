using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class FinancialRecordConfiguration : IEntityTypeConfiguration<FinancialRecord>
{
    public void Configure(EntityTypeBuilder<FinancialRecord> builder)
    {
        builder.ToTable("financial_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Category)
            .HasColumnName("category")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.AccountType)
            .HasColumnName("account_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(r => r.AmountInBs)
            .HasColumnName("amount_in_bs")
            .HasPrecision(18, 2);

        builder.Property(r => r.ExchangeRateValue)
            .HasColumnName("exchange_rate_value")
            .HasPrecision(18, 8);

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.Date)
            .HasColumnName("date");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id");

        builder.Property(r => r.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
