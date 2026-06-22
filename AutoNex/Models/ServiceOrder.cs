using AutoNex.Enums;

namespace AutoNex.Models;

public class ServiceOrder
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public int CurrentKm { get; set; }
    public int? EstimatedDailyKm { get; set; }
    public int? DaysPerWeek { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.Open;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? OperationNumber { get; set; }
    public DateTime? OperationDate { get; set; }
    public decimal? AmountInBs { get; set; }
    public decimal? ExchangeRateValue { get; set; }
    public bool ApplyLaborPercentage { get; set; }
    public decimal? LaborPercentage { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle Vehicle { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public User User { get; set; } = null!;
    public List<ServiceOrderItem> Items { get; set; } = [];
}
