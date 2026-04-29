using ContactsApp.Application.Interfaces;
using ContactsApp.Application.Services;
using ContactsApp.Application.Validators;
using ContactsApp.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ContactsApp.API.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IContactService, ContactService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
