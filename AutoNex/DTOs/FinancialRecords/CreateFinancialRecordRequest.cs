using AutoNex.Enums;

namespace AutoNex.DTOs.FinancialRecords;

public record CreateFinancialRecordRequest(
    FinancialRecordType Type,
    FinancialCategory Category,
    decimal Amount,
    string? Description,
    DateTime Date,
    int UserId
);
