using ContactsApp.Application.DTOs;
using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsApp.API.Controllers;

/// <summary>CRUD operations for contacts.</summary>
[ApiController]
[Route("api/v1/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactService contactService, ILogger<ContactsController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    /// <summary>Get all contacts with pagination, optionally filtered by search term.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _contactService.GetAllAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResponse<ContactDto>>.Ok(result));
    }

    /// <summary>Get a contact by its identifier.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var contact = await _contactService.GetByIdAsync(id);
        return Ok(ApiResponse<ContactDto>.Ok(contact));
    }

    /// <summary>Create a new contact.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        var contact = await _contactService.CreateAsync(dto);
        _logger.LogInformation("Contact created with id {Id}", contact.Id);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ContactDto>.Ok(contact, "Contact created successfully."));
    }

    /// <summary>Update an existing contact.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactDto dto)
    {
        var contact = await _contactService.UpdateAsync(id, dto);
        _logger.LogInformation("Contact updated with id {Id}", id);
        return Ok(ApiResponse<ContactDto>.Ok(contact, "Contact updated successfully."));
    }

    /// <summary>Soft-delete a contact.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _contactService.DeleteAsync(id);
        _logger.LogInformation("Contact deleted with id {Id}", id);
        return NoContent();
    }
}
