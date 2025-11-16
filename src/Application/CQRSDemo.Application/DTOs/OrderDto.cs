namespace CQRSDemo.Application.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
