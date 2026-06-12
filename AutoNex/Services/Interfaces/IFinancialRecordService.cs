using AutoNex.DTOs.FinancialRecords;

namespace AutoNex.Services.Interfaces;

public interface IFinancialRecordService
{
    Task<List<FinancialRecordResponse>> GetAllAsync(DateTime? from, DateTime? to, string? type, string? category);
    Task<FinancialRecordResponse?> GetByIdAsync(int id);
    Task<FinancialRecordResponse> CreateAsync(CreateFinancialRecordRequest request);
    Task<FinancialRecordResponse?> UpdateAsync(int id, UpdateFinancialRecordRequest request);
    Task<bool> DeleteAsync(int id);
    Task<FinancialSummaryResponse> GetSummaryAsync(DateTime? from, DateTime? to);
    Task<List<CategorySummaryResponse>> GetByCategoryAsync(DateTime? from, DateTime? to);
}
