using AutoNex.Data;
using AutoNex.DTOs.Accounts;
using AutoNex.DTOs.FinancialRecords;
using AutoNex.Enums;
using AutoNex.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances(CancellationToken cancellationToken)
    {
        var query = _context.FinancialRecords.AsNoTracking();

        var bolivaresIncome = await query
            .Where(r => r.AccountType == AccountType.Bolivares && r.Type == FinancialRecordType.Income)
            .SumAsync(r => (decimal?)(r.AmountInBs ?? r.Amount), cancellationToken) ?? 0;

        var bolivaresExpenses = await query
            .Where(r => r.AccountType == AccountType.Bolivares && r.Type == FinancialRecordType.Expense)
            .SumAsync(r => (decimal?)(r.AmountInBs ?? r.Amount), cancellationToken) ?? 0;

        var dolaresIncome = await query
            .Where(r => r.AccountType == AccountType.Dolares && r.Type == FinancialRecordType.Income)
            .SumAsync(r => (decimal?)r.Amount, cancellationToken) ?? 0;

        var dolaresExpenses = await query
            .Where(r => r.AccountType == AccountType.Dolares && r.Type == FinancialRecordType.Expense)
            .SumAsync(r => (decimal?)r.Amount, cancellationToken) ?? 0;

        var balances = new List<AccountBalanceDto>
        {
            new(AccountType.Bolivares, bolivaresIncome - bolivaresExpenses, "Bs."),
            new(AccountType.Dolares, dolaresIncome - dolaresExpenses, "USD"),
        };

        return Ok(ApiResponse<List<AccountBalanceDto>>.Ok(balances));
    }

    [HttpGet("{accountType}/transactions")]
    public async Task<IActionResult> GetTransactions(
        string accountType,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountType>(accountType, true, out var parsedType))
            return BadRequest(ApiResponse<object>.Fail("Tipo de cuenta inválido. Use 'Bolivares' o 'Dolares'."));

        var query = _context.FinancialRecords
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.AccountType == parsedType)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt);

        return Ok(ApiResponse<object>.Ok(await query.ToPagedResponseAsync(
            page, pageSize,
            r => new AccountTransactionResponse(
                r.Id,
                r.AccountType,
                r.Type,
                r.Amount,
                r.Description,
                r.Date,
                "FinancialRecord",
                r.Id,
                r.CreatedAt
            ),
            cancellationToken)));
    }
}
