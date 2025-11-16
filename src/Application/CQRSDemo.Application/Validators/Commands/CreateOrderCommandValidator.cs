using CQRSDemo.Application.Commands.Orders;
using FluentValidation;

namespace CQRSDemo.Application.Validators.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters long.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.")
            .LessThanOrEqualTo(100000).WithMessage("Unit price cannot exceed $100,000.");
    }
}
