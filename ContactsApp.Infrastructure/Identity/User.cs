using Microsoft.AspNetCore.Identity;

namespace ContactsApp.Infrastructure.Identity;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
