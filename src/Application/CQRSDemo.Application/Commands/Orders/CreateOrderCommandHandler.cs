using CQRSDemo.Application.Abstractions;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Commands.Orders;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

            var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken)
                ?? throw new NotFoundException(typeof(Customer), command.CustomerId);

            if (!customer.IsActive)
            {
                throw new ValidationException($"Customer {command.CustomerId} is not active and cannot place orders.");
            }

            if (command.Quantity <= 0)
            {
                throw new ValidationException("Quantity must be greater than 0.");
            }

            if (command.UnitPrice <= 0)
            {
                throw new ValidationException("Unit price must be greater than 0.");
            }

            var order = new Order(
                command.CustomerId,
                command.ProductName,
                command.Quantity,
                command.UnitPrice
            );

            await _orderRepository.AddAsync(order, cancellationToken);

            _logger.LogInformation("Successfully created order with ID {OrderId} for customer {CustomerId}", 
                order.Id, command.CustomerId);

            return order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", command.CustomerId);
            throw;
        }
    }
}
