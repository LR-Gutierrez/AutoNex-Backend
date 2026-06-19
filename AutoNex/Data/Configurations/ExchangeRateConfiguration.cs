using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Value).HasPrecision(18, 8);

        builder.HasIndex(r => new { r.CurrencyId, r.CurrencyNewsletterId }).IsUnique();

        builder.HasOne(r => r.Currency)
            .WithMany(c => c.ExchangeRates)
            .HasForeignKey(r => r.CurrencyId);

        builder.HasOne(r => r.Newsletter)
            .WithMany(n => n.ExchangeRates)
            .HasForeignKey(r => r.CurrencyNewsletterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
