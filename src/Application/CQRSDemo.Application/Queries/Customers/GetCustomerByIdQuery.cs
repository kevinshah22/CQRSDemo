using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;

namespace CQRSDemo.Application.Queries.Customers;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto?>;
