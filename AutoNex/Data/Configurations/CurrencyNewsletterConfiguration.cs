using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class CurrencyNewsletterConfiguration : IEntityTypeConfiguration<CurrencyNewsletter>
{
    public void Configure(EntityTypeBuilder<CurrencyNewsletter> builder)
    {
        builder.ToTable("currency_newsletters");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Status).HasConversion<int>();
        builder.HasIndex(n => new { n.PublishedAt, n.IsActive });
    }
}
