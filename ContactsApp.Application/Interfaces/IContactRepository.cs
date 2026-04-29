using ContactsApp.Domain.Entities;

namespace ContactsApp.Application.Interfaces;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllAsync(string? search = null);
    Task<Contact?> GetByIdAsync(int id);
    Task<Contact> CreateAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task DeleteAsync(Contact contact);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
