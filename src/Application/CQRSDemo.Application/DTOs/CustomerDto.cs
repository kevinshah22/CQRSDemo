namespace CQRSDemo.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
