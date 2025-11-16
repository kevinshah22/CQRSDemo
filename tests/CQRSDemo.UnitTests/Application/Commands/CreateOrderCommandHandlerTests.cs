using CQRSDemo.Application.Commands.Orders;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CQRSDemo.UnitTests.Application.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();
        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "Product", 2, 10.99m);
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating order")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), "Product", 2, 10.99m);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Customer with id {command.CustomerId} was not found.");
    }

    [Fact]
    public async Task Handle_WithInactiveCustomer_ShouldThrowValidationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "Product", 2, 10.99m);
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");
        customer.Deactivate();

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Customer {customerId} is not active and cannot place orders.");
    }

    [Fact]
    public async Task Handle_WithInvalidQuantity_ShouldThrowValidationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "Product", 0, 10.99m);
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public async Task Handle_WithInvalidUnitPrice_ShouldThrowValidationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "Product", 2, -10.99m);
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Unit price must be greater than 0.");
    }
}
