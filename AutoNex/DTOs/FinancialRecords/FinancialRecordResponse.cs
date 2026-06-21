using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record FinancialRecordResponse(
    int Id,
    FinancialRecordType Type,
    FinancialCategory Category,
    AccountType AccountType,
    decimal Amount,
    decimal? AmountInBs,
    decimal? ExchangeRateValue,
    string? Description,
    DateTime Date,
    int UserId,
    string UserName,
    DateTime CreatedAt
);
