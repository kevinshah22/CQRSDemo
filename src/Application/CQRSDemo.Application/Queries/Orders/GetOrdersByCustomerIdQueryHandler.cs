using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Queries.Orders;

public class GetOrdersByCustomerIdQueryHandler : IQueryHandler<GetOrdersByCustomerIdQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersByCustomerIdQueryHandler> _logger;

    public GetOrdersByCustomerIdQueryHandler(IOrderRepository orderRepository, ILogger<GetOrdersByCustomerIdQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving orders for customer {CustomerId}", query.CustomerId);

            var orders = await _orderRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

            var orderDtos = orders.Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.ProductName,
                order.Quantity,
                order.UnitPrice,
                order.TotalAmount,
                order.Status.ToString(),
                order.OrderDate,
                order.CreatedAt,
                order.UpdatedAt
            )).ToList().AsReadOnly();

            _logger.LogInformation("Successfully retrieved {Count} orders for customer {CustomerId}", 
                orderDtos.Count, query.CustomerId);

            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", query.CustomerId);
            throw;
        }
    }
}
