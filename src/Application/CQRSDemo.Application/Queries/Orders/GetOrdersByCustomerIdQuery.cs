using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;

namespace CQRSDemo.Application.Queries.Orders;

public record GetOrdersByCustomerIdQuery(Guid CustomerId) : IQuery<IReadOnlyList<OrderDto>>;
