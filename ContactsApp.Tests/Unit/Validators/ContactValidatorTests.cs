using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Application.Interfaces;
using ContactsApp.Application.Services;
using ContactsApp.Application.Validators;
using ContactsApp.Domain.Exceptions;
using FluentValidation.TestHelper;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ContactsApp.Tests.Unit.Validators;

[TestFixture]
public class ContactValidatorTests
{
    // ------------------------------------------------------------------ CreateContactValidator

    private CreateContactValidator _createValidator = null!;
    private UpdateContactValidator _updateValidator = null!;

    [SetUp]
    public void SetUp()
    {
        _createValidator = new CreateContactValidator();
        _updateValidator = new UpdateContactValidator();
    }

    // ------------------------------------------------------------------ CreateContactValidator - FirstName

    [Test]
    public void CreateContactValidator_WhenFirstNameEmpty_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Le nom est obligatoire");
    }

    [Test]
    public void CreateContactValidator_WhenFirstNameWhiteSpace_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "   ",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void CreateContactValidator_WhenFirstNameExceeds100Chars_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = new string('A', 101),
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    // ------------------------------------------------------------------ CreateContactValidator - LastName

    [Test]
    public void CreateContactValidator_WhenLastNameEmpty_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "",
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Le prénom est obligatoire");
    }

    [Test]
    public void CreateContactValidator_WhenLastNameExceeds100Chars_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = new string('B', 101),
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    // ------------------------------------------------------------------ CreateContactValidator - Email

    [Test]
    public void CreateContactValidator_WhenEmailInvalid_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "not-an-email"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email invalide");
    }

    [Test]
    public void CreateContactValidator_WhenEmailEmpty_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = ""
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email invalide");
    }

    // ------------------------------------------------------------------ CreateContactValidator - Phone

    [Test]
    public void CreateContactValidator_WhenPhoneExceeds20Chars_ShouldFail()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = new string('0', 21)
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Test]
    public void CreateContactValidator_WhenPhoneIsNull_ShouldPass()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = null
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    // ------------------------------------------------------------------ CreateContactValidator - Valid

    [Test]
    public void CreateContactValidator_WhenValid_ShouldPass()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = "0600000000"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void CreateContactValidator_WhenValidWithoutPhone_ShouldPass()
    {
        var dto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _createValidator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ------------------------------------------------------------------ UpdateContactValidator - same rules

    [Test]
    public void UpdateContactValidator_WhenFirstNameEmpty_ShouldFail()
    {
        var dto = new UpdateContactDto
        {
            FirstName = "",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _updateValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("Le nom est obligatoire");
    }

    [Test]
    public void UpdateContactValidator_WhenEmailInvalid_ShouldFail()
    {
        var dto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "bad-email"
        };

        var result = _updateValidator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email invalide");
    }

    [Test]
    public void UpdateContactValidator_WhenValid_ShouldPass()
    {
        var dto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var result = _updateValidator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ------------------------------------------------------------------ ContactService - email conflict (via service)

    [Test]
    public void CreateContactValidator_WhenEmailAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var contactRepositoryMock = new Mock<IContactRepository>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger<ContactService>>();

        var sut = new ContactService(
            contactRepositoryMock.Object,
            mapperMock.Object,
            loggerMock.Object);

        var createDto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        contactRepositoryMock
            .Setup(x => x.EmailExistsAsync(createDto.Email, null))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.That(
            async () => await sut.CreateAsync(createDto),
            Throws.TypeOf<ConflictException>());
    }
}
