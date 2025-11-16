using CQRSDemo.Application.Abstractions;

namespace CQRSDemo.Application.Commands.Customers;

public record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
) : ICommand;
