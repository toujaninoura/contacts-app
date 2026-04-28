using ContactsApp.Application.DTOs.Contacts;

namespace ContactsApp.Application.Interfaces;

public interface IContactService
{
    Task<IEnumerable<ContactDto>> GetAllAsync(string? search = null);
    Task<ContactDto> GetByIdAsync(int id);
    Task<ContactDto> CreateAsync(CreateContactDto dto);
    Task<ContactDto> UpdateAsync(int id, UpdateContactDto dto);
    Task DeleteAsync(int id);
}
