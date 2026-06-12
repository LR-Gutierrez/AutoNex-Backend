using AutoNex.Enums;

namespace AutoNex.DTOs.InventoryMovements;

public record InventoryMovementResponse(
    int Id,
    int? ConsumableId,
    string? ConsumableName,
    int? ToolId,
    string? ToolName,
    MovementType MovementType,
    int Quantity,
    string? Reference,
    int? ReferenceId,
    string? Notes,
    DateTime CreatedAt
);
