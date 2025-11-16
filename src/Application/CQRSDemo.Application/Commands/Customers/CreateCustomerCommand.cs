using CQRSDemo.Application.Abstractions;

namespace CQRSDemo.Application.Commands.Customers;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
) : ICommand<Guid>;
