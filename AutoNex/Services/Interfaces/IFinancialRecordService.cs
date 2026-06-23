using AutoNex.DTOs;
using AutoNex.DTOs.FinancialRecords;

namespace AutoNex.Services.Interfaces;

public interface IFinancialRecordService
{
    Task<PagedResponse<FinancialRecordResponse>> GetAllAsync(string? search, DateTime? from, DateTime? to, string? type, string? category, int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<FinancialRecordResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FinancialRecordResponse> CreateAsync(CreateFinancialRecordRequest request, int userId, CancellationToken cancellationToken = default);
    Task<FinancialRecordResponse?> UpdateAsync(int id, UpdateFinancialRecordRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<FinancialSummaryResponse> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<List<CategorySummaryResponse>> GetByCategoryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<List<DailySummaryResponse>> GetDailySummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
