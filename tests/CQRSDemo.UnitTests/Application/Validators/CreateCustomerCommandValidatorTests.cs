using CQRSDemo.Application.Commands.Customers;
using CQRSDemo.Application.Validators.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace CQRSDemo.UnitTests.Application.Validators;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    [Fact]
    public void Validation_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john.doe@example.com", "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validation_WithInvalidFirstName_ShouldHaveError(string firstName)
    {
        // Arrange
        var command = new CreateCustomerCommand(firstName, "Doe", "john.doe@example.com", "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validation_WithInvalidLastName_ShouldHaveError(string lastName)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", lastName, "john.doe@example.com", "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validation_WithInvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", email, "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("abc")]
    public void Validation_WithInvalidPhoneNumber_ShouldHaveError(string phoneNumber)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john.doe@example.com", phoneNumber);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validation_WithFirstNameTooLong_ShouldHaveError()
    {
        // Arrange
        var longFirstName = new string('A', 101);
        var command = new CreateCustomerCommand(longFirstName, "Doe", "john.doe@example.com", "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validation_WithLastNameTooLong_ShouldHaveError()
    {
        // Arrange
        var longLastName = new string('A', 101);
        var command = new CreateCustomerCommand("John", longLastName, "john.doe@example.com", "+1234567890");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
