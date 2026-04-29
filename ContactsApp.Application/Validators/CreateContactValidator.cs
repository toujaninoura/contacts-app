using ContactsApp.Application.DTOs.Contacts;
using FluentValidation;

namespace ContactsApp.Application.Validators;

public class CreateContactValidator : AbstractValidator<CreateContactDto>
{
    public CreateContactValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => x.Phone is not null);
    }
}
