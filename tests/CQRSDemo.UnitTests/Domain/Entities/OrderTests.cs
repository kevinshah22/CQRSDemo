using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CQRSDemo.UnitTests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 5;
        var unitPrice = 10.99m;

        // Act
        var order = new Order(customerId, productName, quantity, unitPrice);

        // Assert
        order.CustomerId.Should().Be(customerId);
        order.ProductName.Should().Be(productName);
        order.Quantity.Should().Be(quantity);
        order.UnitPrice.Should().Be(unitPrice);
        order.TotalAmount.Should().Be(quantity * unitPrice);
        order.Status.Should().Be(OrderStatus.Pending);
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_ShouldUpdateStatus()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "Product", 1, 10.99m);

        // Act
        order.UpdateStatus(OrderStatus.Processing);

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateProduct_WithPendingOrder_ShouldUpdateProduct()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "Product", 1, 10.99m);

        // Act
        order.UpdateProduct("New Product", 2, 20.99m);

        // Assert
        order.ProductName.Should().Be("New Product");
        order.Quantity.Should().Be(2);
        order.UnitPrice.Should().Be(20.99m);
        order.TotalAmount.Should().Be(41.98m);
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateProduct_WithCompletedOrder_ShouldThrowException()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "Product", 1, 10.99m);
        order.UpdateStatus(OrderStatus.Completed);

        // Act & Assert
        var act = () => order.UpdateProduct("New Product", 2, 20.99m);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot update a completed or cancelled order.");
    }

    [Fact]
    public void UpdateProduct_WithCancelledOrder_ShouldThrowException()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "Product", 1, 10.99m);
        order.UpdateStatus(OrderStatus.Cancelled);

        // Act & Assert
        var act = () => order.UpdateProduct("New Product", 2, 20.99m);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot update a completed or cancelled order.");
    }
}
