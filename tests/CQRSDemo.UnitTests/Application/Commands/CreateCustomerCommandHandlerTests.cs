using CQRSDemo.Application.Commands.Customers;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CQRSDemo.UnitTests.Application.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _handler = new CreateCustomerCommandHandler(_customerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john.doe@example.com", "+1234567890");
        _customerRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Creating new customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john.doe@example.com", "+1234567890");
        var existingCustomer = new Customer("Jane", "Smith", "john.doe@example.com", "+0987654321");
        
        _customerRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("A customer with email john.doe@example.com already exists.");
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john.doe@example.com", "+1234567890");
        var exception = new Exception("Database error");

        _customerRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating customer")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
