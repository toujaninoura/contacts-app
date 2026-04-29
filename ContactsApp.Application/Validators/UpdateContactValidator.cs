using ContactsApp.Application.DTOs.Contacts;
using FluentValidation;

namespace ContactsApp.Application.Validators;

public class UpdateContactValidator : AbstractValidator<UpdateContactDto>
{
    public UpdateContactValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le nom est obligatoire")
            .MaximumLength(100).WithMessage("Le nom ne doit pas dépasser 100 caractères");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le prénom est obligatoire")
            .MaximumLength(100).WithMessage("Le prénom ne doit pas dépasser 100 caractères");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email invalide")
            .EmailAddress().WithMessage("Email invalide")
            .MaximumLength(255).WithMessage("L'email ne doit pas dépasser 255 caractères");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Le téléphone ne doit pas dépasser 20 caractères")
            .When(x => x.Phone is not null);
    }
}
