using System.Net.Http.Json;
using CQRSDemo.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CQRSDemo.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public OrdersControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    private async Task<CustomerDto> CreateTestCustomer()
    {
        var request = new
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test.customer@example.com",
            PhoneNumber = "+1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturnCreatedOrder()
    {
        // Arrange
        var customer = await CreateTestCustomer();
        var request = new
        {
            CustomerId = customer.Id,
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = 10.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(request.CustomerId);
        order.ProductName.Should().Be(request.ProductName);
        order.Quantity.Should().Be(request.Quantity);
        order.UnitPrice.Should().Be(request.UnitPrice);
        order.TotalAmount.Should().Be(request.Quantity * request.UnitPrice);
        order.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentCustomer_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            CustomerId = Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = 10.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidQuantity_ShouldReturnBadRequest()
    {
        // Arrange
        var customer = await CreateTestCustomer();
        var request = new
        {
            CustomerId = customer.Id,
            ProductName = "Test Product",
            Quantity = 0,
            UnitPrice = 10.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidUnitPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var customer = await CreateTestCustomer();
        var request = new
        {
            CustomerId = customer.Id,
            ProductName = "Test Product",
            Quantity = 2,
            UnitPrice = -10.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrdersByCustomerId_WithExistingOrders_ShouldReturnOrders()
    {
        // Arrange
        var customer = await CreateTestCustomer();
        
        var order1Request = new
        {
            CustomerId = customer.Id,
            ProductName = "Product 1",
            Quantity = 1,
            UnitPrice = 10.99m
        };

        var order2Request = new
        {
            CustomerId = customer.Id,
            ProductName = "Product 2",
            Quantity = 2,
            UnitPrice = 20.99m
        };

        await _client.PostAsJsonAsync("/api/orders", order1Request);
        await _client.PostAsJsonAsync("/api/orders", order2Request);

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().Be(2);
        orders.All(o => o.CustomerId == customer.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetOrdersByCustomerId_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var customer = await CreateTestCustomer();

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetOrdersByCustomerId_WithNonExistentCustomer_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{nonExistentCustomerId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Count.Should().Be(0);
    }
}
