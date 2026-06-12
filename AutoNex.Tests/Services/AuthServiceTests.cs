using AutoNex.DTOs.Auth;
using AutoNex.Enums;
using AutoNex.Services.Implementations;
using AutoNex.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace AutoNex.Tests.Services;

public class AuthServiceTests
{
    private static IConfiguration CreateConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestKeyThatIsAtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();

        return config;
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUser()
    {
        var context = TestDbContextFactory.Create();
        var service = new AuthService(context, CreateConfig());

        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Test123",
            Role = UserRole.Mechanic
        };

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.FullName, result.FullName);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var context = TestDbContextFactory.Create();
        var service = new AuthService(context, CreateConfig());

        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Test123",
            Role = UserRole.Mechanic
        };

        await service.RegisterAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var context = TestDbContextFactory.Create();
        var service = new AuthService(context, CreateConfig());

        var registerRequest = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Test123",
            Role = UserRole.Mechanic
        };

        await service.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest("test@example.com", "Test123");
        var result = await service.LoginAsync(loginRequest);

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var context = TestDbContextFactory.Create();
        var service = new AuthService(context, CreateConfig());

        var registerRequest = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Test123",
            Role = UserRole.Mechanic
        };

        await service.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest("test@example.com", "WrongPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        var context = TestDbContextFactory.Create();
        var service = new AuthService(context, CreateConfig());

        var request = new LoginRequest("nonexistent@example.com", "Test123");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(request));
    }
}
