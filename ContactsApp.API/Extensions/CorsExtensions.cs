namespace ContactsApp.API.Extensions;

public static class CorsExtensions
{
    public const string ANGULAR_POLICY = "AllowAngular";

    public static IServiceCollection AddAngularCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(ANGULAR_POLICY, policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
