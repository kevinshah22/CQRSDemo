using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.Commands.Orders;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Application.Queries.Orders;
using Microsoft.AspNetCore.Mvc;

namespace CQRSDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public OrdersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByCustomerId(Guid customerId)
    {
        var orders = await _dispatcher.Dispatch(new GetOrdersByCustomerIdQuery(customerId));
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.ProductName,
            request.Quantity,
            request.UnitPrice);

        var orderId = await _dispatcher.Dispatch(command);

        return CreatedAtAction(
            nameof(GetByCustomerId), 
            new { customerId = request.CustomerId }, 
            orderId);
    }
}

public record CreateOrderRequest(
    Guid CustomerId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
