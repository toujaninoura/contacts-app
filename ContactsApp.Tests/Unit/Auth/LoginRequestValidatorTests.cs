using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Validators;
using FluentValidation.TestHelper;

namespace ContactsApp.Tests.Unit.Auth;

[TestFixture]
public class LoginRequestValidatorTests
{
    private LoginRequestValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LoginRequestValidator();
    }

    [Test]
    public void Validate_WhenValidRequest_ShouldHaveNoErrors()
    {
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_WhenEmailInvalid_ShouldHaveError()
    {
        var dto = new LoginRequestDto
        {
            Email = "bad-email",
            Password = "Password1!"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenEmailEmpty_ShouldHaveError()
    {
        var dto = new LoginRequestDto
        {
            Email = "",
            Password = "Password1!"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_WhenPasswordEmpty_ShouldHaveError()
    {
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = ""
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
