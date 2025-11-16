using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CQRSDemo.UnitTests.Domain.Entities;

public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateCustomer()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+1234567890";

        // Act
        var customer = new Customer(firstName, lastName, email, phoneNumber);

        // Assert
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().Be(email);
        customer.PhoneNumber.Should().Be(phoneNumber);
        customer.IsActive.Should().BeTrue();
        customer.Id.Should().NotBeEmpty();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCustomer()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");
        var originalCreatedAt = customer.CreatedAt;

        // Act
        customer.Update("Jane", "Smith", "jane.smith@example.com", "+0987654321");

        // Assert
        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.Email.Should().Be("jane.smith@example.com");
        customer.PhoneNumber.Should().Be("+0987654321");
        customer.CreatedAt.Should().Be(originalCreatedAt);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");
        customer.Deactivate();

        // Act
        customer.Activate();

        // Assert
        customer.IsActive.Should().BeTrue();
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddOrder_ShouldAddOrderToCustomer()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");
        var order = new Order(customer.Id, "Product", 1, 10.99m);

        // Act
        customer.AddOrder(order);

        // Assert
        customer.Orders.Should().Contain(order);
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
