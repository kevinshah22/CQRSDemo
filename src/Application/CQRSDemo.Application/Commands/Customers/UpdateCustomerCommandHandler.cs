using CQRSDemo.Application.Abstractions;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Exceptions;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Commands.Customers;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(ICustomerRepository customerRepository, ILogger<UpdateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating customer with ID {CustomerId}", command.Id);

            var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException(typeof(Customer), command.Id);

            customer.Update(command.FirstName, command.LastName, command.Email, command.PhoneNumber);

            await _customerRepository.UpdateAsync(customer, cancellationToken);

            _logger.LogInformation("Successfully updated customer with ID {CustomerId}", command.Id);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID {CustomerId}", command.Id);
            throw;
        }
    }
}
