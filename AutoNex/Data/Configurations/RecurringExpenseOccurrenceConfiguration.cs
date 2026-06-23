using AutoNex.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoNex.Data.Configurations;

public class RecurringExpenseOccurrenceConfiguration : IEntityTypeConfiguration<RecurringExpenseOccurrence>
{
    public void Configure(EntityTypeBuilder<RecurringExpenseOccurrence> builder)
    {
        builder.ToTable("recurring_expense_occurrences");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.RecurringExpenseId)
            .HasColumnName("recurring_expense_id");

        builder.Property(o => o.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date");

        builder.Property(o => o.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(o => o.IsPaid)
            .HasColumnName("is_paid");

        builder.Property(o => o.PaidDate)
            .HasColumnName("paid_date");

        builder.Property(o => o.PaidAccountType)
            .HasColumnName("paid_account_type")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.FinancialRecordId)
            .HasColumnName("financial_record_id");

        builder.Property(o => o.DismissedDate)
            .HasColumnName("dismissed_date")
            .HasColumnType("date");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at");

        builder.HasQueryFilter(o => !o.RecurringExpense.IsDeleted);

        builder.HasOne(o => o.RecurringExpense)
            .WithMany(e => e.Occurrences)
            .HasForeignKey(o => o.RecurringExpenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.FinancialRecord)
            .WithMany()
            .HasForeignKey(o => o.FinancialRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
