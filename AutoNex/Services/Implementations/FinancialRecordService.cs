using AutoNex.Data;
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

    public async Task<List<FinancialRecordResponse>> GetAllAsync(DateTime? from, DateTime? to, string? type, string? category)
    {
        var query = _context.FinancialRecords
            .Include(r => r.User)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.Date <= to.Value);
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<FinancialRecordType>(type, true, out var parsedType))
            query = query.Where(r => r.Type == parsedType);
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<FinancialCategory>(category, true, out var parsedCategory))
            query = query.Where(r => r.Category == parsedCategory);

        var records = await query
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return records.Select(r => r.ToResponse()).ToList();
    }

    public async Task<FinancialRecordResponse?> GetByIdAsync(int id)
    {
        var record = await _context.FinancialRecords
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        return record?.ToResponse();
    }

    public async Task<FinancialRecordResponse> CreateAsync(CreateFinancialRecordRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId)
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        var record = new FinancialRecord
        {
            Type = request.Type,
            Category = request.Category,
            Amount = request.Amount,
            Description = request.Description,
            Date = request.Date,
            UserId = request.UserId
        };

        _context.FinancialRecords.Add(record);
        await _context.SaveChangesAsync();

        return record.ToResponse();
    }

    public async Task<FinancialRecordResponse?> UpdateAsync(int id, UpdateFinancialRecordRequest request)
    {
        var record = await _context.FinancialRecords.FindAsync(id);
        if (record is null) return null;

        record.Type = request.Type;
        record.Category = request.Category;
        record.Amount = request.Amount;
        record.Description = request.Description;
        record.Date = request.Date;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return record.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var record = await _context.FinancialRecords.FindAsync(id);
        if (record is null) return false;

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FinancialSummaryResponse> GetSummaryAsync(DateTime? from, DateTime? to)
    {
        var query = _context.FinancialRecords.AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.Date <= to.Value);

        var records = await query.ToListAsync();

        return new FinancialSummaryResponse(
            records.Where(r => r.Type == FinancialRecordType.Income).Sum(r => r.Amount),
            records.Where(r => r.Type == FinancialRecordType.Expense).Sum(r => r.Amount),
            records.Where(r => r.Type == FinancialRecordType.Income).Sum(r => r.Amount) -
                records.Where(r => r.Type == FinancialRecordType.Expense).Sum(r => r.Amount),
            records.Count(r => r.Type == FinancialRecordType.Income),
            records.Count(r => r.Type == FinancialRecordType.Expense)
        );
    }

    public async Task<List<CategorySummaryResponse>> GetByCategoryAsync(DateTime? from, DateTime? to)
    {
        var query = _context.FinancialRecords.AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.Date <= to.Value);

        var grouped = await query
            .GroupBy(r => r.Category)
            .Select(g => new CategorySummaryResponse(
                g.Key.ToString(),
                g.Sum(r => r.Amount),
                g.Count()
            ))
            .OrderByDescending(g => g.TotalAmount)
            .ToListAsync();

        return grouped;
    }
}
