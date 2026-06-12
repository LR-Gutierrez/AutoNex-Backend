using AutoNex.DTOs.Clients;
using AutoNex.Services.Implementations;
using AutoNex.Tests.Helpers;

namespace AutoNex.Tests.Services;

public class ClientServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesClient()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var request = new CreateClientRequest("John Doe", "123456789", "john@example.com", "123 Main St");

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.FullName, result.FullName);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsClient()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var created = await service.CreateAsync(new CreateClientRequest("John Doe", "123456789", "john@example.com", "123 Main St"));
        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("John Doe", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesClient()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var created = await service.CreateAsync(new CreateClientRequest("John Doe", "123456789", "john@example.com", "123 Main St"));

        var result = await service.UpdateAsync(created.Id, new UpdateClientRequest("Jane Doe", "987654321", "jane@example.com", "456 Oak Ave"));

        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.FullName);
        Assert.Equal("jane@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var result = await service.UpdateAsync(999, new UpdateClientRequest("Jane Doe", "987654321", "jane@example.com", "456 Oak Ave"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_SoftDeletesClient()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var created = await service.CreateAsync(new CreateClientRequest("John Doe", "123456789", "john@example.com", "123 Main St"));
        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);

        var result = await service.GetByIdAsync(created.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        var context = TestDbContextFactory.Create();
        var service = new ClientService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }
}
