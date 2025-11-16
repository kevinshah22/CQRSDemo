using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.Commands.Customers;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Application.Queries.Customers;
using Microsoft.AspNetCore.Mvc;

namespace CQRSDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public CustomersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll()
    {
        var customers = await _dispatcher.Dispatch(new GetAllCustomersQuery());
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var customer = await _dispatcher.Dispatch(new GetCustomerByIdQuery(id));
        
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCustomerRequest request)
    {
        var command = new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var customerId = await _dispatcher.Dispatch(command);

        return CreatedAtAction(nameof(GetById), new { id = customerId }, customerId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        await _dispatcher.Dispatch(command);

        return NoContent();
    }
}

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);

public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);
