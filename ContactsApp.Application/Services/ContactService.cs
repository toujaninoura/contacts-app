using AutoMapper;
using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Entities;
using ContactsApp.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ContactsApp.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IContactRepository contactRepository,
        IMapper mapper,
        ILogger<ContactService> logger)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ContactDto>> GetAllAsync(string? search = null)
    {
        var contacts = await _contactRepository.GetAllAsync(search);
        return _mapper.Map<IEnumerable<ContactDto>>(contacts);
    }

    public async Task<ContactDto> GetByIdAsync(int id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        if (contact is null)
            throw new NotFoundException(nameof(Contact), id);

        return _mapper.Map<ContactDto>(contact);
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto dto)
    {
        var emailExists = await _contactRepository.EmailExistsAsync(dto.Email, null);
        if (emailExists)
            throw new ConflictException($"Email '{dto.Email}' is already used by another contact.");

        var contact = _mapper.Map<Contact>(dto);
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        var created = await _contactRepository.CreateAsync(contact);
        _logger.LogInformation("Contact created with id {Id}", created.Id);

        return _mapper.Map<ContactDto>(created);
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactDto dto)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        if (contact is null)
            throw new NotFoundException(nameof(Contact), id);

        var emailExists = await _contactRepository.EmailExistsAsync(dto.Email, id);
        if (emailExists)
            throw new ConflictException($"Email '{dto.Email}' is already used by another contact.");

        _mapper.Map(dto, contact);
        contact.UpdatedAt = DateTime.UtcNow;

        var updated = await _contactRepository.UpdateAsync(contact);
        _logger.LogInformation("Contact updated with id {Id}", id);

        return _mapper.Map<ContactDto>(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        if (contact is null)
            throw new NotFoundException(nameof(Contact), id);

        await _contactRepository.DeleteAsync(contact);
        _logger.LogInformation("Contact soft-deleted with id {Id}", id);
    }
}
