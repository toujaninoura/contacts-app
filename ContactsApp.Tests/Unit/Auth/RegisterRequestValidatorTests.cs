using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Validators;
using FluentValidation.TestHelper;

namespace ContactsApp.Tests.Unit.Auth;

[TestFixture]
public class RegisterRequestValidatorTests
{
    private RegisterRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterRequestValidator();
    }

    [Test]
    public void Validate_WhenValidRequest_ShouldHaveNoErrors()
    {
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WhenEmailInvalid_ShouldHaveError()
    {
        var dto = new RegisterRequestDto
        {
            Email = "not-an-email",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenEmailEmpty_ShouldHaveError()
    {
        var dto = new RegisterRequestDto
        {
            Email = "",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenPasswordTooShort_ShouldHaveError()
    {
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Short1",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Validate_WhenFirstNameEmpty_ShouldHaveError()
    {
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "",
            LastName = "Dupont"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Validate_WhenLastNameEmpty_ShouldHaveError()
    {
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = ""
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
