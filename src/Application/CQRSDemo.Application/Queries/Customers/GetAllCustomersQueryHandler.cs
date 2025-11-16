using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;
using CQRSDemo.Domain.Entities;
using CQRSDemo.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CQRSDemo.Application.Queries.Customers;

public class GetAllCustomersQueryHandler : IQueryHandler<GetAllCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<GetAllCustomersQueryHandler> _logger;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository, ILogger<GetAllCustomersQueryHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(GetAllCustomersQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all customers");

            var customers = await _customerRepository.GetAllAsync(cancellationToken);

            var customerDtos = customers.Select(customer => new CustomerDto(
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.IsActive,
                customer.CreatedAt,
                customer.UpdatedAt
            )).ToList().AsReadOnly();

            _logger.LogInformation("Successfully retrieved {Count} customers", customerDtos.Count);

            return customerDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            throw;
        }
    }
}
