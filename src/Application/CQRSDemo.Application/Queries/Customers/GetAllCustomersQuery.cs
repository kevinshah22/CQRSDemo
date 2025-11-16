using CQRSDemo.Application.Abstractions;
using CQRSDemo.Application.DTOs;

namespace CQRSDemo.Application.Queries.Customers;

public record GetAllCustomersQuery() : IQuery<IReadOnlyList<CustomerDto>>;
