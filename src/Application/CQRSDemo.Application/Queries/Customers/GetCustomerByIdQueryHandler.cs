using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Queries.Customers;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository, ILogger<GetCustomerByIdQueryHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving customer with ID {CustomerId}", query.Id);

            var customer = await _customerRepository.GetByIdAsync(query.Id, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found", query.Id);
                return null;
            }

            var customerDto = new CustomerDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.IsActive,
                customer.CreatedAt,
                customer.UpdatedAt
            );

            _logger.LogInformation("Successfully retrieved customer with ID {CustomerId}", query.Id);

            return customerDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", query.Id);
            throw;
        }
    }
}
