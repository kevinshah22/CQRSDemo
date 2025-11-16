using CQRSDemo.Application.Abstractions;

namespace CQRSDemo.Application.Commands.Orders;

public record CreateOrderCommand(
    Guid CustomerId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
) : ICommand<Guid>;
