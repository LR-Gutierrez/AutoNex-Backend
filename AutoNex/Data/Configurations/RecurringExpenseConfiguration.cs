using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class RecurringExpenseConfiguration : IEntityTypeConfiguration<RecurringExpense>
{
    public void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        builder.ToTable("recurring_expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(e => e.Frequency)
            .HasColumnName("frequency")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.DayOfMonth)
            .HasColumnName("day_of_month");

        builder.Property(e => e.AccountType)
            .HasColumnName("account_type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
