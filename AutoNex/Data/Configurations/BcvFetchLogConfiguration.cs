using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class BcvFetchLogConfiguration : IEntityTypeConfiguration<BcvFetchLog>
{
    public void Configure(EntityTypeBuilder<BcvFetchLog> builder)
    {
        builder.ToTable("bcv_fetch_logs");

        builder.Property(l => l.ValueDate)
            .HasColumnName("value_date")
            .HasColumnType("date");

        builder.Property(l => l.RatesJson)
            .HasColumnName("rates_json")
            .HasColumnType("jsonb");

        builder.Property(l => l.IsSuccess)
            .HasColumnName("is_success");

        builder.Property(l => l.Error)
            .HasColumnName("error");

        builder.Property(l => l.Action)
            .HasColumnName("action")
            .HasMaxLength(50);

        builder.Property(l => l.FetchedBy)
            .HasColumnName("fetched_by")
            .HasMaxLength(20);

        builder.Property(l => l.FetchedAt)
            .HasColumnName("fetched_at");
    }
}
