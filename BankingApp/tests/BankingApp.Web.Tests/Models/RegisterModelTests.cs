namespace BankingApp.Web.Tests.Models;

using System.ComponentModel.DataAnnotations;
using BankingApp.Web.Models;

public sealed class RegisterModelTests
{
    [Fact]
    public void RegisterModel_WhenAllFieldsAreValid_ShouldPassValidation()
    {
        // Arrange
        RegisterModel model = CreateValidModel();

        // Act
        List<ValidationResult> results = Validate(model);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void RegisterModel_WhenEmailIsInvalid_ShouldFailValidation()
    {
        // Arrange
        RegisterModel model = CreateValidModel();
        model.Email = "not-an-email";

        // Act
        List<ValidationResult> results = Validate(model);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.Email)));
    }

    [Fact]
    public void RegisterModel_WhenPasswordIsWeak_ShouldFailValidation()
    {
        // Arrange
        RegisterModel model = CreateValidModel();
        model.Password = "weakpass";
        model.PasswordConfirmation = "weakpass";

        // Act
        List<ValidationResult> results = Validate(model);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.Password)));
    }

    [Fact]
    public void RegisterModel_WhenPasswordConfirmationDoesNotMatch_ShouldFailValidation()
    {
        // Arrange
        RegisterModel model = CreateValidModel();
        model.PasswordConfirmation = "DifferentPassword1!";

        // Act
        List<ValidationResult> results = Validate(model);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.PasswordConfirmation)));
    }

    [Fact]
    public void RegisterModel_WhenRequiredFieldsAreEmpty_ShouldFailValidation()
    {
        // Arrange
        RegisterModel model = new();

        // Act
        List<ValidationResult> results = Validate(model);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.FullName)));
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.Email)));
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.Password)));
        results.Should().Contain(result => result.MemberNames.Contains(nameof(RegisterModel.PasswordConfirmation)));
    }

    private static RegisterModel CreateValidModel()
    {
        return new RegisterModel
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "StrongPassword1!",
            PasswordConfirmation = "StrongPassword1!"
        };
    }

    private static List<ValidationResult> Validate(RegisterModel model)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        return results;
    }
}
