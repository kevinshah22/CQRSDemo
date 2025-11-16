using CQRSDemo.Application.Abstractions;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Commands.Customers;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository, ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new customer with email {Email}", command.Email);

            var existingCustomer = await _customerRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingCustomer != null)
            {
                throw new ValidationException($"A customer with email {command.Email} already exists.");
            }

            var customer = new Customer(
                command.FirstName,
                command.LastName,
                command.Email,
                command.PhoneNumber
            );

            await _customerRepository.AddAsync(customer, cancellationToken);

            _logger.LogInformation("Successfully created customer with ID {CustomerId}", customer.Id);

            return customer.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with email {Email}", command.Email);
            throw;
        }
    }
}
