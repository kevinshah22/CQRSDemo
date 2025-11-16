using CQRSDemo.Application.DTOs;
using CQRSDemo.Application.Queries.Customers;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CQRSDemo.UnitTests.Application.Queries;

public class GetCustomerByIdQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ILogger<GetCustomerByIdQueryHandler>> _loggerMock;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<GetCustomerByIdQueryHandler>>();
        _handler = new GetCustomerByIdQueryHandler(_customerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnCustomerDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe", "john.doe@example.com", "+1234567890");
        var query = new GetCustomerByIdQuery(customerId);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be(customer.FirstName);
        result.LastName.Should().Be(customer.LastName);
        result.Email.Should().Be(customer.Email);
        result.PhoneNumber.Should().Be(customer.PhoneNumber);
        result.IsActive.Should().Be(customer.IsActive);
        result.CreatedAt.Should().Be(customer.CreatedAt);
        result.UpdatedAt.Should().Be(customer.UpdatedAt);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieving customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieving customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);
        var exception = new Exception("Database error");

        _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving customer")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
