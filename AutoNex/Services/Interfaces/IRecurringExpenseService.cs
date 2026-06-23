using AutoNex.DTOs.RecurringExpenses;

namespace AutoNex.Services.Interfaces;

public interface IRecurringExpenseService
{
    Task<List<RecurringExpenseResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RecurringExpenseResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RecurringExpenseResponse> CreateAsync(CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default);
    Task<RecurringExpenseResponse?> UpdateAsync(int id, UpdateRecurringExpenseRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<RecurringExpenseOccurrenceResponse>> GetDueTodayAsync(CancellationToken cancellationToken = default);
    Task<RecurringExpenseOccurrenceResponse?> PayOccurrenceAsync(int occurrenceId, PayRecurringExpenseRequest request, int userId, CancellationToken cancellationToken = default);
    Task<RecurringExpenseOccurrenceResponse?> DismissOccurrenceAsync(int occurrenceId, CancellationToken cancellationToken = default);
    Task GenerateOccurrencesAsync(CancellationToken cancellationToken = default);
}
