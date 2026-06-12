using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record UpdateFinancialRecordRequest(
    FinancialRecordType Type,
    FinancialCategory Category,
    decimal Amount,
    string? Description,
    DateTime Date
);
