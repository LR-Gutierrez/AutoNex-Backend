using System.Security.Claims;
using AutoNex.DTOs;
using AutoNex.DTOs.RecurringExpenses;
using AutoNex.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/recurring-expenses")]
[Authorize]
public class RecurringExpensesController : ControllerBase
{
    private readonly IRecurringExpenseService _recurringExpenseService;
    private readonly IDashboardNotifier _dashboardNotifier;
    private readonly INotificationService _notificationService;

    public RecurringExpensesController(
        IRecurringExpenseService recurringExpenseService,
        IDashboardNotifier dashboardNotifier,
        INotificationService notificationService)
    {
        _recurringExpenseService = recurringExpenseService;
        _dashboardNotifier = dashboardNotifier;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var expenses = await _recurringExpenseService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<RecurringExpenseResponse>>.Ok(expenses));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var expense = await _recurringExpenseService.GetByIdAsync(id, cancellationToken);
        if (expense is null)
            return NotFound(ApiResponse<RecurringExpenseResponse>.Fail("Gasto recurrente no encontrado"));
        return Ok(ApiResponse<RecurringExpenseResponse>.Ok(expense));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await _recurringExpenseService.CreateAsync(request, cancellationToken);
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id },
            ApiResponse<RecurringExpenseResponse>.Ok(expense, "Gasto recurrente creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = await _recurringExpenseService.UpdateAsync(id, request, cancellationToken);
        if (expense is null)
            return NotFound(ApiResponse<RecurringExpenseResponse>.Fail("Gasto recurrente no encontrado"));
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<RecurringExpenseResponse>.Ok(expense, "Gasto recurrente actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _recurringExpenseService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponse<bool>.Fail("Gasto recurrente no encontrado"));
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Gasto recurrente eliminado exitosamente"));
    }

    [HttpGet("due-today")]
    public async Task<IActionResult> GetDueToday(CancellationToken cancellationToken)
    {
        var due = await _recurringExpenseService.GetDueTodayAsync(cancellationToken);
        return Ok(ApiResponse<List<RecurringExpenseOccurrenceResponse>>.Ok(due));
    }

    [HttpPost("occurrences/{occurrenceId}/pay")]
    public async Task<IActionResult> PayOccurrence(int occurrenceId, [FromBody] PayRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _recurringExpenseService.PayOccurrenceAsync(occurrenceId, request, userId, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<RecurringExpenseOccurrenceResponse>.Fail("Ocurrencia no encontrada"));
        await _dashboardNotifier.NotifyAllAsync(cancellationToken);
        return Ok(ApiResponse<RecurringExpenseOccurrenceResponse>.Ok(result, "Gasto registrado como pagado exitosamente"));
    }

    [HttpPost("occurrences/{occurrenceId}/dismiss")]
    public async Task<IActionResult> DismissOccurrence(int occurrenceId, CancellationToken cancellationToken)
    {
        var result = await _recurringExpenseService.DismissOccurrenceAsync(occurrenceId, cancellationToken);
        if (result is null)
            return NotFound(ApiResponse<RecurringExpenseOccurrenceResponse>.Fail("Ocurrencia no encontrada"));
        return Ok(ApiResponse<RecurringExpenseOccurrenceResponse>.Ok(result, "Notificación descartada por hoy"));
    }
}
