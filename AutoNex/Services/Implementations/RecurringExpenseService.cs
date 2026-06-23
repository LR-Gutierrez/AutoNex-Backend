using AutoNex.Data;
using AutoNex.DTOs.RecurringExpenses;
using AutoNex.Enums;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class RecurringExpenseService : IRecurringExpenseService
{
    private readonly AppDbContext _context;

    public RecurringExpenseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecurringExpenseResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RecurringExpenses
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => MapToResponse(e))
            .ToListAsync(cancellationToken);
    }

    public async Task<RecurringExpenseResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RecurringExpenses
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity is null ? null : MapToResponse(entity);
    }

    public async Task<RecurringExpenseResponse> CreateAsync(CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new RecurringExpense
        {
            Name = request.Name,
            Amount = request.Amount,
            Frequency = request.Frequency,
            DayOfMonth = request.DayOfMonth,
            AccountType = request.AccountType,
            Type = request.Type,
            Description = request.Description,
        };

        _context.RecurringExpenses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    public async Task<RecurringExpenseResponse?> UpdateAsync(int id, UpdateRecurringExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RecurringExpenses
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null) return null;

        entity.Name = request.Name;
        entity.Amount = request.Amount;
        entity.Frequency = request.Frequency;
        entity.DayOfMonth = request.DayOfMonth;
        entity.AccountType = request.AccountType;
        entity.Type = request.Type;
        entity.IsActive = request.IsActive;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RecurringExpenses
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null) return false;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<RecurringExpenseOccurrenceResponse>> GetDueTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var due = await _context.RecurringExpenseOccurrences
            .AsNoTracking()
            .Include(o => o.RecurringExpense)
            .Where(o => o.DueDate <= today && !o.IsPaid && (o.DismissedDate == null || o.DismissedDate < today))
            .OrderBy(o => o.DueDate)
            .ThenBy(o => o.RecurringExpense.Name)
            .Select(o => MapOccurrenceToResponse(o))
            .ToListAsync(cancellationToken);

        return due;
    }

    public async Task<RecurringExpenseOccurrenceResponse?> PayOccurrenceAsync(int occurrenceId, PayRecurringExpenseRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var occurrence = await _context.RecurringExpenseOccurrences
            .Include(o => o.RecurringExpense)
            .FirstOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken);

        if (occurrence is null) return null;
        if (occurrence.IsPaid) return MapOccurrenceToResponse(occurrence);

        var isIncome = occurrence.RecurringExpense.Type == FinancialRecordType.Income;
        var financialRecord = new FinancialRecord
        {
            Type = occurrence.RecurringExpense.Type,
            Category = FinancialCategory.Other,
            AccountType = request.AccountType,
            Amount = occurrence.Amount,
            Description = $"{(isIncome ? "Ingreso" : "Gasto")} recurrente: {occurrence.RecurringExpense.Name}",
            Date = DateTime.UtcNow,
            UserId = userId
        };

        _context.FinancialRecords.Add(financialRecord);
        await _context.SaveChangesAsync(cancellationToken);

        occurrence.IsPaid = true;
        occurrence.PaidDate = DateTime.UtcNow;
        occurrence.PaidAccountType = request.AccountType;
        occurrence.FinancialRecordId = financialRecord.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return MapOccurrenceToResponse(occurrence);
    }

    public async Task<RecurringExpenseOccurrenceResponse?> DismissOccurrenceAsync(int occurrenceId, CancellationToken cancellationToken = default)
    {
        var occurrence = await _context.RecurringExpenseOccurrences
            .Include(o => o.RecurringExpense)
            .FirstOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken);

        if (occurrence is null) return null;

        occurrence.DismissedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);

        return MapOccurrenceToResponse(occurrence);
    }

    public async Task GenerateOccurrencesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var horizon = today.AddDays(90);
        var activeExpenses = await _context.RecurringExpenses
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var expense in activeExpenses)
        {
            var dueDates = GetDueDates(expense, today, horizon);

            foreach (var dueDate in dueDates)
            {
                if (dueDate < today) continue;

                var exists = await _context.RecurringExpenseOccurrences
                    .AnyAsync(o => o.RecurringExpenseId == expense.Id && o.DueDate == dueDate, cancellationToken);

                if (!exists)
                {
                    _context.RecurringExpenseOccurrences.Add(new RecurringExpenseOccurrence
                    {
                        RecurringExpenseId = expense.Id,
                        DueDate = dueDate,
                        Amount = expense.Amount
                    });
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static List<DateOnly> GetDueDates(RecurringExpense expense, DateOnly from, DateOnly to)
    {
        var dates = new List<DateOnly>();

        switch (expense.Frequency)
        {
            case RecurringExpenseFrequency.Daily:
                for (var d = from; d <= to; d = d.AddDays(1))
                    dates.Add(d);
                break;

            case RecurringExpenseFrequency.Weekly:
            {
                var isoDay = expense.DayOfMonth; // 1=Monday..7=Sunday
                var targetDw = isoDay == 7 ? 0 : isoDay;
                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    if ((int)d.DayOfWeek == targetDw)
                        dates.Add(d);
                }
                break;
            }

            case RecurringExpenseFrequency.Biweekly:
            {
                var start = DateOnly.FromDateTime(expense.CreatedAt);
                var next = start;
                while (next <= to)
                {
                    if (next >= from)
                        dates.Add(next);
                    next = next.AddDays(14);
                }
                break;
            }

            case RecurringExpenseFrequency.Monthly:
            {
                var cursor = new DateOnly(from.Year, from.Month, 1);
                while (cursor <= to)
                {
                    var day = Math.Min(expense.DayOfMonth, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                    var due = new DateOnly(cursor.Year, cursor.Month, day);
                    if (due >= from)
                        dates.Add(due);
                    cursor = cursor.AddMonths(1);
                }
                break;
            }

            case RecurringExpenseFrequency.Bimonthly:
            {
                var created = DateOnly.FromDateTime(expense.CreatedAt);
                var cursor = new DateOnly(from.Year, from.Month, 1);
                while (cursor <= to)
                {
                    var monthsSince = ((cursor.Year - created.Year) * 12) + cursor.Month - created.Month;
                    if (monthsSince >= 0 && monthsSince % 2 == 0)
                    {
                        var day = Math.Min(expense.DayOfMonth, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                        var due = new DateOnly(cursor.Year, cursor.Month, day);
                        if (due >= from)
                            dates.Add(due);
                    }
                    cursor = cursor.AddMonths(1);
                }
                break;
            }

            case RecurringExpenseFrequency.Quarterly:
            {
                var created = DateOnly.FromDateTime(expense.CreatedAt);
                var cursor = new DateOnly(from.Year, from.Month, 1);
                while (cursor <= to)
                {
                    var monthsSince = ((cursor.Year - created.Year) * 12) + cursor.Month - created.Month;
                    if (monthsSince >= 0 && monthsSince % 3 == 0)
                    {
                        var day = Math.Min(expense.DayOfMonth, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                        var due = new DateOnly(cursor.Year, cursor.Month, day);
                        if (due >= from)
                            dates.Add(due);
                    }
                    cursor = cursor.AddMonths(1);
                }
                break;
            }

            case RecurringExpenseFrequency.Yearly:
            {
                var created = DateOnly.FromDateTime(expense.CreatedAt);
                var cursor = new DateOnly(from.Year, from.Month, 1);
                while (cursor <= to)
                {
                    if (cursor.Month == created.Month)
                    {
                        var day = Math.Min(expense.DayOfMonth, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                        var due = new DateOnly(cursor.Year, cursor.Month, day);
                        if (due >= from)
                            dates.Add(due);
                    }
                    cursor = cursor.AddMonths(1);
                }
                break;
            }
        }

        return dates;
    }

    private static RecurringExpenseResponse MapToResponse(RecurringExpense entity)
    {
        return new RecurringExpenseResponse(
            entity.Id,
            entity.Name,
            entity.Amount,
            entity.Frequency,
            entity.DayOfMonth,
            entity.AccountType,
            entity.Type,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt
        );
    }

    private static RecurringExpenseOccurrenceResponse MapOccurrenceToResponse(RecurringExpenseOccurrence occurrence)
    {
        return new RecurringExpenseOccurrenceResponse(
            occurrence.Id,
            occurrence.RecurringExpenseId,
            occurrence.RecurringExpense?.Name ?? "",
            occurrence.DueDate,
            occurrence.Amount,
            occurrence.IsPaid,
            occurrence.PaidDate,
            occurrence.PaidAccountType,
            occurrence.DismissedDate
        );
    }
}
