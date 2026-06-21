using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class FinancialRecordService : IFinancialRecordService
{
    private readonly AppDbContext _context;

    public FinancialRecordService(AppDbContext context)
    {
        _context = context;
    }

    private static DateTime? ToUtc(DateTime? dt) =>
        dt.HasValue ? (dt.Value.Kind == DateTimeKind.Utc ? dt.Value : dt.Value.ToUniversalTime()) : null;

    public async Task<PagedResponse<FinancialRecordResponse>> GetAllAsync(DateTime? from, DateTime? to, string? type, string? category, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords
            .AsNoTracking()
            .Include(r => r.User)
            .AsQueryable();

        var utcFrom = ToUtc(from);
        var utcTo = ToUtc(to);

        if (utcFrom.HasValue)
            query = query.Where(r => r.Date >= utcFrom.Value);
        if (utcTo.HasValue)
            query = query.Where(r => r.Date <= utcTo.Value);
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<FinancialRecordType>(type, true, out var parsedType))
            query = query.Where(r => r.Type == parsedType);
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<FinancialCategory>(category, true, out var parsedCategory))
            query = query.Where(r => r.Category == parsedCategory);

        query = query
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt);

        return await query.ToPagedResponseAsync(page, pageSize, r => r.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<FinancialRecordResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _context.FinancialRecords
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return record?.ToResponse();
    }

    public async Task<FinancialRecordResponse> CreateAsync(CreateFinancialRecordRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var record = new FinancialRecord
        {
            Type = request.Type,
            Category = request.Category,
            AccountType = request.AccountType,
            Amount = request.Amount,
            Description = request.Description,
            Date = ToUtc(request.Date) ?? request.Date,
            UserId = userId
        };

        _context.FinancialRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return record.ToResponse();
    }

    public async Task<FinancialRecordResponse?> UpdateAsync(int id, UpdateFinancialRecordRequest request, CancellationToken cancellationToken = default)
    {
        var record = await _context.FinancialRecords
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (record is null) return null;

        record.Type = request.Type;
        record.Category = request.Category;
        record.AccountType = request.AccountType;
        record.Amount = request.Amount;
        record.Description = request.Description;
        record.Date = ToUtc(request.Date) ?? request.Date;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return record.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _context.FinancialRecords.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (record is null) return false;

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<FinancialSummaryResponse> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords.AsNoTracking().AsQueryable();

        var utcFrom = ToUtc(from);
        var utcTo = ToUtc(to);

        if (utcFrom.HasValue)
            query = query.Where(r => r.Date >= utcFrom.Value);
        if (utcTo.HasValue)
            query = query.Where(r => r.Date <= utcTo.Value);

        var incomeSum = await query.Where(r => r.Type == FinancialRecordType.Income).SumAsync(r => (decimal?)r.Amount, cancellationToken).ConfigureAwait(false) ?? 0;
        var expenseSum = await query.Where(r => r.Type == FinancialRecordType.Expense).SumAsync(r => (decimal?)r.Amount, cancellationToken).ConfigureAwait(false) ?? 0;
        var incomeCount = await query.Where(r => r.Type == FinancialRecordType.Income).CountAsync(cancellationToken).ConfigureAwait(false);
        var expenseCount = await query.Where(r => r.Type == FinancialRecordType.Expense).CountAsync(cancellationToken).ConfigureAwait(false);

        var bolivaresIncome = await query.Where(r => r.AccountType == AccountType.Bolivares && r.Type == FinancialRecordType.Income).SumAsync(r => (decimal?)(r.AmountInBs ?? r.Amount), cancellationToken).ConfigureAwait(false) ?? 0;
        var bolivaresExpenses = await query.Where(r => r.AccountType == AccountType.Bolivares && r.Type == FinancialRecordType.Expense).SumAsync(r => (decimal?)(r.AmountInBs ?? r.Amount), cancellationToken).ConfigureAwait(false) ?? 0;
        var dolaresIncome = await query.Where(r => r.AccountType == AccountType.Dolares && r.Type == FinancialRecordType.Income).SumAsync(r => (decimal?)r.Amount, cancellationToken).ConfigureAwait(false) ?? 0;
        var dolaresExpenses = await query.Where(r => r.AccountType == AccountType.Dolares && r.Type == FinancialRecordType.Expense).SumAsync(r => (decimal?)r.Amount, cancellationToken).ConfigureAwait(false) ?? 0;

        var balances = new List<AccountBalanceDto>
        {
            new(AccountType.Bolivares, bolivaresIncome - bolivaresExpenses, "Bs."),
            new(AccountType.Dolares, dolaresIncome - dolaresExpenses, "USD"),
        };

        return new FinancialSummaryResponse(
            incomeSum,
            expenseSum,
            incomeSum - expenseSum,
            incomeCount,
            expenseCount,
            balances
        );
    }

    public async Task<List<CategorySummaryResponse>> GetByCategoryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialRecords.AsNoTracking().AsQueryable();

        var utcFrom = ToUtc(from);
        var utcTo = ToUtc(to);

        if (utcFrom.HasValue)
            query = query.Where(r => r.Date >= utcFrom.Value);
        if (utcTo.HasValue)
            query = query.Where(r => r.Date <= utcTo.Value);

        var grouped = await query
            .GroupBy(r => r.Category)
            .Select(g => new
            {
                Category = g.Key,
                TotalAmount = g.Sum(r => r.Amount),
                Count = g.Count()
            })
            .OrderByDescending(g => g.TotalAmount)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => new CategorySummaryResponse(
            g.Category.ToString(),
            g.TotalAmount,
            g.Count
        )).ToList();
    }
}
