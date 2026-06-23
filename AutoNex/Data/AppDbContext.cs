using AutoNex.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Consumable> Consumables => Set<Consumable>();
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<ServiceOrderItem> ServiceOrderItems => Set<ServiceOrderItem>();
    public DbSet<MileageAlert> MileageAlerts => Set<MileageAlert>();
    public DbSet<FinancialRecord> FinancialRecords => Set<FinancialRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyNewsletter> CurrencyNewsletters => Set<CurrencyNewsletter>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<BcvFetchLog> BcvFetchLogs => Set<BcvFetchLog>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();
    public DbSet<RecurringExpenseOccurrence> RecurringExpenseOccurrences => Set<RecurringExpenseOccurrence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
