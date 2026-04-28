using ContactsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContactsApp.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Contacts.IgnoreQueryFilters().AnyAsync())
            {
                var contacts = new List<Contact>
                {
                    new Contact
                    {
                        FirstName = "Alice",
                        LastName = "Martin",
                        Email = "alice.martin@example.com",
                        Phone = "+33612345678",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Contact
                    {
                        FirstName = "Bob",
                        LastName = "Dupont",
                        Email = "bob.dupont@example.com",
                        Phone = "+33623456789",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Contact
                    {
                        FirstName = "Claire",
                        LastName = "Bernard",
                        Email = "claire.bernard@example.com",
                        Phone = "+33634567890",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Contact
                    {
                        FirstName = "David",
                        LastName = "Leroy",
                        Email = "david.leroy@example.com",
                        Phone = "+33645678901",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Contact
                    {
                        FirstName = "Emma",
                        LastName = "Petit",
                        Email = "emma.petit@example.com",
                        Phone = "+33656789012",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    }
                };

                await context.Contacts.AddRangeAsync(contacts);
                await context.SaveChangesAsync();
                logger.LogInformation("Seed data inserted: {Count} contacts created.", contacts.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
