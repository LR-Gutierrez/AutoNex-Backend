using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record UpdateFinancialRecordRequest(
    FinancialRecordType Type,
    FinancialCategory Category,
    AccountType AccountType,
    decimal Amount,
    string? Description,
    DateTime Date
);
