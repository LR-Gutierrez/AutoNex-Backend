using AutoNex.Enums;

namespace AutoNex.DTOs.Accounts;

public record AccountTransactionResponse(
    int Id,
    AccountType AccountType,
    FinancialRecordType Type,
    decimal Amount,
    string? Description,
    DateTime Date,
    string? ReferenceType,
    int? ReferenceId,
    DateTime CreatedAt
);
