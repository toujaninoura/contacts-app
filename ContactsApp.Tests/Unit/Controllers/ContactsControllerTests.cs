using ContactsApp.API.Controllers;
using ContactsApp.Application.DTOs;
using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactsApp.Tests.Unit.Controllers;

[TestFixture]
public class ContactsControllerTests
{
    private Mock<IContactService> _contactServiceMock = null!;
    private Mock<ILogger<ContactsController>> _loggerMock = null!;
    private ContactsController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _contactServiceMock = new Mock<IContactService>();
        _loggerMock = new Mock<ILogger<ContactsController>>();
        _sut = new ContactsController(_contactServiceMock.Object, _loggerMock.Object);
    }

    // ------------------------------------------------------------------ GetAll

    [Test]
    public async Task GetAll_WhenCalled_ShouldReturn200WithPagedResponse()
    {
        // Arrange
        var pagedResponse = new PagedResponse<ContactDto>
        {
            Data = new List<ContactDto>
            {
                new() { Id = 1, FirstName = "Alice", LastName = "Dupont", Email = "alice@example.com" }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _contactServiceMock
            .Setup(x => x.GetAllAsync(1, 10, null))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _sut.GetAll(1, 10, null);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var apiResponse = okResult.Value as ApiResponse<PagedResponse<ContactDto>>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data, Is.Not.Null);
        Assert.That(apiResponse.Data!.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAll_WithSearch_ShouldPassSearchToService()
    {
        // Arrange
        const string search = "Alice";
        var pagedResponse = new PagedResponse<ContactDto>
        {
            Data = new List<ContactDto>
            {
                new() { Id = 1, FirstName = "Alice", LastName = "Dupont", Email = "alice@example.com" }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _contactServiceMock
            .Setup(x => x.GetAllAsync(1, 10, search))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _sut.GetAll(1, 10, search);

        // Assert
        _contactServiceMock.Verify(x => x.GetAllAsync(1, 10, search), Times.Once);
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task GetAll_WhenNoContacts_ShouldReturn200WithEmptyList()
    {
        // Arrange
        var pagedResponse = new PagedResponse<ContactDto>
        {
            Data = Enumerable.Empty<ContactDto>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        _contactServiceMock
            .Setup(x => x.GetAllAsync(1, 10, null))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _sut.GetAll(1, 10, null);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        var apiResponse = okResult!.Value as ApiResponse<PagedResponse<ContactDto>>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Data!.TotalCount, Is.EqualTo(0));
        Assert.That(apiResponse.Data.Data.Count(), Is.EqualTo(0));
    }

    // ------------------------------------------------------------------ GetById

    [Test]
    public async Task GetById_WhenContactFound_ShouldReturn200WithContactDto()
    {
        // Arrange
        var contactDto = new ContactDto
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactServiceMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(contactDto);

        // Act
        var result = await _sut.GetById(1);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var apiResponse = okResult.Value as ApiResponse<ContactDto>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data!.Id, Is.EqualTo(1));
        Assert.That(apiResponse.Data.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void GetById_WhenContactNotFound_ShouldPropagateNotFoundException()
    {
        // Arrange
        _contactServiceMock
            .Setup(x => x.GetByIdAsync(999))
            .ThrowsAsync(new NotFoundException("Contact", 999));

        // Act & Assert
        Assert.That(
            async () => await _sut.GetById(999),
            Throws.TypeOf<NotFoundException>());
    }

    // ------------------------------------------------------------------ Create

    [Test]
    public async Task Create_WhenValidRequest_ShouldReturn201WithContactDto()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = "0600000000"
        };

        var contactDto = new ContactDto
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = "0600000000"
        };

        _contactServiceMock
            .Setup(x => x.CreateAsync(createDto))
            .ReturnsAsync(contactDto);

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        var statusResult = result as ObjectResult;
        Assert.That(statusResult, Is.Not.Null);
        Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

        var apiResponse = statusResult.Value as ApiResponse<ContactDto>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data!.Id, Is.EqualTo(1));
        Assert.That(apiResponse.Data.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void Create_WhenEmailAlreadyExists_ShouldPropagateConflictException()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactServiceMock
            .Setup(x => x.CreateAsync(createDto))
            .ThrowsAsync(new ConflictException("Email already in use."));

        // Act & Assert
        Assert.That(
            async () => await _sut.Create(createDto),
            Throws.TypeOf<ConflictException>());
    }

    // ------------------------------------------------------------------ Update

    [Test]
    public async Task Update_WhenValidRequest_ShouldReturn200WithUpdatedDto()
    {
        // Arrange
        const int id = 1;
        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice Updated",
            LastName = "Dupont",
            Email = "alice.updated@example.com"
        };

        var contactDto = new ContactDto
        {
            Id = id,
            FirstName = "Alice Updated",
            LastName = "Dupont",
            Email = "alice.updated@example.com"
        };

        _contactServiceMock
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ReturnsAsync(contactDto);

        // Act
        var result = await _sut.Update(id, updateDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var apiResponse = okResult.Value as ApiResponse<ContactDto>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data!.FirstName, Is.EqualTo("Alice Updated"));
    }

    [Test]
    public void Update_WhenContactNotFound_ShouldPropagateNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactServiceMock
            .Setup(x => x.UpdateAsync(999, updateDto))
            .ThrowsAsync(new NotFoundException("Contact", 999));

        // Act & Assert
        Assert.That(
            async () => await _sut.Update(999, updateDto),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void Update_WhenEmailConflict_ShouldPropagateConflictException()
    {
        // Arrange
        const int id = 1;
        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "taken@example.com"
        };

        _contactServiceMock
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ThrowsAsync(new ConflictException("Email already in use by another contact."));

        // Act & Assert
        Assert.That(
            async () => await _sut.Update(id, updateDto),
            Throws.TypeOf<ConflictException>());
    }

    // ------------------------------------------------------------------ Delete

    [Test]
    public async Task Delete_WhenContactFound_ShouldReturn204NoContent()
    {
        // Arrange
        _contactServiceMock
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        Assert.That(noContentResult!.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        _contactServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Test]
    public void Delete_WhenContactNotFound_ShouldPropagateNotFoundException()
    {
        // Arrange
        _contactServiceMock
            .Setup(x => x.DeleteAsync(999))
            .ThrowsAsync(new NotFoundException("Contact", 999));

        // Act & Assert
        Assert.That(
            async () => await _sut.Delete(999),
            Throws.TypeOf<NotFoundException>());
    }
}
