using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.ServiceOrders;
using AutoNex.Enums;
using AutoNex.Models;
using AutoNex.Services.Implementations;
using AutoNex.Services.Interfaces;
using AutoNex.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoNex.Tests.Services;

public class ServiceOrderServiceTests
{
    private static async Task<AppDbContext> CreateSeedContextAsync()
    {
        var context = TestDbContextFactory.Create();

        context.Clients.Add(new Client
        {
            Id = 1,
            FullName = "Test Client",
            Phone = "04121234567"
        });

        context.Vehicles.Add(new Vehicle
        {
            Id = 1,
            ClientId = 1,
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2020,
            LicensePlate = "ABC123"
        });

        context.Services.Add(new Service
        {
            Id = 1,
            Name = "Cambio de Aceite",
            DefaultPrice = 45.00m
        });

        context.Consumables.Add(new Consumable
        {
            Id = 1,
            Name = "Aceite 5W-30",
            StockQuantity = 10,
            MinStock = 2,
            UnitPrice = 25.00m,
            Category = ConsumableCategory.Oil
        });

        context.Users.Add(new User
        {
            Id = 1,
            FullName = "Test User",
            Email = "test@autonex.com",
            PasswordHash = "hash",
            Role = UserRole.Mechanic,
            IsActive = true
        });

        await context.SaveChangesAsync();
        return context;
    }

    private static ServiceOrderService CreateService(AppDbContext context)
    {
        var alertService = new MockMileageAlertService();
        var exchangeRateService = Mock.Of<IExchangeRateService>();
        var logger = Mock.Of<ILogger<ServiceOrderService>>();
        return new ServiceOrderService(context, alertService, exchangeRateService, logger);
    }

    [Fact]
    public async Task CreateAsync_WithServiceItem_CreatesOrder()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var request = new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items =
            [
                new CreateServiceOrderItemRequest
                {
                    Type = "Service",
                    ServiceId = 1,
                    Quantity = 1,
                    UnitPrice = 45.00m
                }
            ]
        };

        var result = await service.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(45.00m, result.TotalAmount);
        Assert.Equal(ServiceOrderStatus.Open, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithConsumable_DeductsStock()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var request = new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items =
            [
                new CreateServiceOrderItemRequest
                {
                    Type = "Consumable",
                    ConsumableId = 1,
                    Quantity = 3,
                    UnitPrice = 25.00m
                }
            ]
        };

        await service.CreateAsync(request, 1);

        var consumable = await context.Consumables.FindAsync(1);
        Assert.Equal(7, consumable!.StockQuantity);
    }

    [Fact]
    public async Task CreateAsync_WithInsufficientStock_Throws()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var request = new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items =
            [
                new CreateServiceOrderItemRequest
                {
                    Type = "Consumable",
                    ConsumableId = 1,
                    Quantity = 99,
                    UnitPrice = 25.00m
                }
            ]
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(request, 1));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentVehicle_Throws()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var request = new CreateServiceOrderRequest
        {
            VehicleId = 999,
            ClientId = 1,
            CurrentKm = 50000,
            Items = []
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(request, 1));
    }

    [Fact]
    public async Task UpdateStatusAsync_Cancel_RestoresStock()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var createRequest = new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items =
            [
                new CreateServiceOrderItemRequest
                {
                    Type = "Consumable",
                    ConsumableId = 1,
                    Quantity = 2,
                    UnitPrice = 25.00m
                }
            ]
        };

        var created = await service.CreateAsync(createRequest, 1);
        Assert.Equal(8, (await context.Consumables.FindAsync(1))!.StockQuantity);

        await service.UpdateStatusAsync(created.Id, new UpdateServiceOrderStatusRequest
        {
            Status = ServiceOrderStatus.Cancelled
        });

        Assert.Equal(10, (await context.Consumables.FindAsync(1))!.StockQuantity);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatus()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var request = new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items =
            [
                new CreateServiceOrderItemRequest
                {
                    Type = "Service",
                    ServiceId = 1,
                    Quantity = 1,
                    UnitPrice = 45.00m
                }
            ]
        };

        await service.CreateAsync(request, 1);

        var result = await service.GetAllAsync(null, null, null, null, "Open", 1, 10);
        Assert.Single(result.Items);

        var emptyResult = await service.GetAllAsync(null, null, null, null, "Completed", 1, 10);
        Assert.Empty(emptyResult.Items);
    }

    [Fact]
    public async Task PayAsync_WithBolivaresAndNoRate_Throws()
    {
        var context = await CreateSeedContextAsync();
        var alertService = new MockMileageAlertService();
        var exchangeRateService = Mock.Of<IExchangeRateService>();
        var logger = Mock.Of<ILogger<ServiceOrderService>>();
        var service = new ServiceOrderService(context, alertService, exchangeRateService, logger);

        var created = await service.CreateAsync(new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items = [new CreateServiceOrderItemRequest { Type = "Service", ServiceId = 1, Quantity = 1, UnitPrice = 45 }]
        }, 1);

        await service.UpdateStatusAsync(created.Id, new UpdateServiceOrderStatusRequest { Status = ServiceOrderStatus.Completed });

        Mock.Get(exchangeRateService)
            .Setup(x => x.GetLatestValueByCodeAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PayAsync(created.Id, 1, new PayOrderRequest(PaymentMethod.EfectivoBolivares, null, null, 2500)));
    }

    [Fact]
    public async Task PayAsync_WithBolivaresAndRate_CreatesRecord()
    {
        var context = await CreateSeedContextAsync();
        var alertService = new MockMileageAlertService();
        var exchangeRateService = Mock.Of<IExchangeRateService>();
        var logger = Mock.Of<ILogger<ServiceOrderService>>();
        var service = new ServiceOrderService(context, alertService, exchangeRateService, logger);

        var created = await service.CreateAsync(new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items = [new CreateServiceOrderItemRequest { Type = "Service", ServiceId = 1, Quantity = 1, UnitPrice = 45 }]
        }, 1);

        await service.UpdateStatusAsync(created.Id, new UpdateServiceOrderStatusRequest { Status = ServiceOrderStatus.Completed });

        Mock.Get(exchangeRateService)
            .Setup(x => x.GetLatestValueByCodeAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(50.25m);

        var result = await service.PayAsync(created.Id, 1, new PayOrderRequest(PaymentMethod.EfectivoBolivares, null, null, 2500));

        Assert.Equal(ServiceOrderStatus.Paid, result.Status);

        var record = await context.FinancialRecords.FirstOrDefaultAsync();
        Assert.NotNull(record);
        Assert.Equal(45m, record.Amount);
        Assert.Equal(2500m, record.AmountInBs);
        Assert.Equal(50.25m, record.ExchangeRateValue);
    }

    [Fact]
    public async Task PayAsync_WithUsdPayment_NoExchangeRateFields()
    {
        var context = await CreateSeedContextAsync();
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateServiceOrderRequest
        {
            VehicleId = 1,
            ClientId = 1,
            CurrentKm = 50000,
            Items = [new CreateServiceOrderItemRequest { Type = "Service", ServiceId = 1, Quantity = 1, UnitPrice = 45 }]
        }, 1);

        await service.UpdateStatusAsync(created.Id, new UpdateServiceOrderStatusRequest { Status = ServiceOrderStatus.Completed });

        var result = await service.PayAsync(created.Id, 1, new PayOrderRequest(PaymentMethod.EfectivoDolares, null, null));

        Assert.Equal(ServiceOrderStatus.Paid, result.Status);

        var record = await context.FinancialRecords.FirstOrDefaultAsync();
        Assert.NotNull(record);
        Assert.Null(record.AmountInBs);
        Assert.Null(record.ExchangeRateValue);
    }
}

public class MockMileageAlertService : IMileageAlertService
{
    public Task<PagedResponse<DTOs.MileageAlerts.MileageAlertResponse>> GetAllAsync(bool? due, int? page, int? pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagedResponse<DTOs.MileageAlerts.MileageAlertResponse>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        });

    public Task<DTOs.MileageAlerts.MileageAlertResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult<DTOs.MileageAlerts.MileageAlertResponse?>(null);

    public Task<DTOs.MileageAlerts.MileageAlertResponse> CreateAsync(DTOs.MileageAlerts.CreateMileageAlertRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<DTOs.MileageAlerts.MileageAlertResponse?> UpdateAsync(int id, DTOs.MileageAlerts.UpdateMileageAlertRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<List<DTOs.MileageAlerts.MileageAlertResponse>> CreateOrUpdateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<DTOs.MileageAlerts.MileageAlertResponse>());

    public Task<DTOs.MileageAlerts.MileageAlertResponse?> AttendAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
