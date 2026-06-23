using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record CreateFinancialRecordRequest(
    FinancialRecordType Type,
    FinancialCategory Category,
    AccountType AccountType,
    decimal Amount,
    decimal? AmountInBs,
    decimal? ExchangeRateValue,
    string? Description,
    DateTime Date
);
