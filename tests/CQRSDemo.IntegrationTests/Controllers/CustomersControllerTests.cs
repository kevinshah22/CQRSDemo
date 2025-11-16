using System.Net.Http.Json;
using CQRSDemo.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CQRSDemo.IntegrationTests.Controllers;

public class CustomersControllerTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly HttpClient _client;

    public CustomersControllerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_WithValidRequest_ShouldReturnCreatedCustomer()
    {
        // Arrange
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be(request.FirstName);
        customer.LastName.Should().Be(request.LastName);
        customer.Email.Should().Be(request.Email);
        customer.PhoneNumber.Should().Be(request.PhoneNumber);
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            PhoneNumber = "+1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerById_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        var createRequest = new
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "+0987654321"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        // Act
        var response = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.FirstName.Should().Be(createRequest.FirstName);
        customer.LastName.Should().Be(createRequest.LastName);
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllCustomers_WithMultipleCustomers_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1Request = new
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@example.com",
            PhoneNumber = "+1111111111"
        };

        var customer2Request = new
        {
            FirstName = "Bob",
            LastName = "Brown",
            Email = "bob.brown@example.com",
            PhoneNumber = "+2222222222"
        };

        await _client.PostAsJsonAsync("/api/customers", customer1Request);
        await _client.PostAsJsonAsync("/api/customers", customer2Request);

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
        customers!.Count.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidRequest_ShouldUpdateCustomer()
    {
        // Arrange
        var createRequest = new
        {
            FirstName = "Charlie",
            LastName = "Wilson",
            Email = "charlie.wilson@example.com",
            PhoneNumber = "+3333333333"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var updateRequest = new
        {
            Id = createdCustomer!.Id,
            FirstName = "Charlie Updated",
            LastName = "Wilson Updated",
            Email = "charlie.updated@example.com",
            PhoneNumber = "+4444444444"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{createdCustomer.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        
        // Verify the update
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        var updatedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        
        updatedCustomer!.FirstName.Should().Be(updateRequest.FirstName);
        updatedCustomer.LastName.Should().Be(updateRequest.LastName);
        updatedCustomer.Email.Should().Be(updateRequest.Email);
        updatedCustomer.PhoneNumber.Should().Be(updateRequest.PhoneNumber);
    }
}
