using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Entities;
using ContactsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactsApp.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;

    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetAllAsync(string? search = null)
    {
        var query = _context.Contacts
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term));
        }

        return await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();
        return contact;
    }

    public async Task DeleteAsync(Contact contact)
    {
        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Contacts.AsNoTracking()
            .Where(c => c.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
