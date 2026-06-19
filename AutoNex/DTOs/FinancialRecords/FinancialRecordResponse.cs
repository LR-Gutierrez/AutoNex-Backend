using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record FinancialRecordResponse(
    int Id,
    FinancialRecordType Type,
    FinancialCategory Category,
    decimal Amount,
    decimal? AmountInBs,
    decimal? ExchangeRateValue,
    string? Description,
    DateTime Date,
    int UserId,
    string UserName,
    DateTime CreatedAt
);
