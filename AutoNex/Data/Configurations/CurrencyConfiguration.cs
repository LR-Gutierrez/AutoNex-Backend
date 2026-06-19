using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.IsoCode).HasMaxLength(3).IsRequired();
        builder.HasIndex(c => c.IsoCode).IsUnique();
        builder.Property(c => c.Symbol).HasMaxLength(10);
    }
}
