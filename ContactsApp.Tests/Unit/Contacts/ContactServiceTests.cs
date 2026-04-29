using AutoMapper;
using ContactsApp.Application.DTOs;
using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Application.Interfaces;
using ContactsApp.Application.Services;
using ContactsApp.Domain.Entities;
using ContactsApp.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactsApp.Tests.Unit.Contacts;

[TestFixture]
public class ContactServiceTests
{
    private Mock<IContactRepository> _contactRepositoryMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private Mock<ILogger<ContactService>> _loggerMock = null!;
    private IContactService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _contactRepositoryMock = new Mock<IContactRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ContactService>>();

        _sut = new ContactService(
            _contactRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    // ------------------------------------------------------------------ GetAllAsync

    [Test]
    public async Task GetAllAsync_WhenNoContacts_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        _contactRepositoryMock
            .Setup(x => x.GetAllAsync(null))
            .ReturnsAsync(new List<Contact>());

        // Act
        var result = await _sut.GetAllAsync(1, 10, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(0));
        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.TotalPages, Is.EqualTo(0));
        Assert.That(result.HasNext, Is.False);
        Assert.That(result.HasPrev, Is.False);
    }

    [Test]
    public async Task GetAllAsync_WhenContactsExist_ShouldReturnPagedResponse()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Dupont", Email = "alice@example.com" },
            new() { Id = 2, FirstName = "Bob", LastName = "Martin", Email = "bob@example.com" }
        };

        _contactRepositoryMock
            .Setup(x => x.GetAllAsync(null))
            .ReturnsAsync(contacts);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(It.IsAny<Contact>()))
            .Returns((Contact c) => new ContactDto { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email });

        // Act
        var result = await _sut.GetAllAsync(1, 10, null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Page, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 15)
            .Select(i => new Contact { Id = i, FirstName = $"User{i}", LastName = "Test", Email = $"user{i}@example.com" })
            .ToList();

        _contactRepositoryMock
            .Setup(x => x.GetAllAsync(null))
            .ReturnsAsync(contacts);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(It.IsAny<Contact>()))
            .Returns((Contact c) => new ContactDto { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email });

        // Act
        var result = await _sut.GetAllAsync(2, 10, null);

        // Assert
        Assert.That(result.Data.Count(), Is.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(15));
        Assert.That(result.TotalPages, Is.EqualTo(2));
        Assert.That(result.HasNext, Is.False);
        Assert.That(result.HasPrev, Is.True);
    }

    [Test]
    public async Task GetAllAsync_WithSearch_ShouldPassSearchToRepository()
    {
        // Arrange
        const string search = "Alice";
        var contacts = new List<Contact>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Dupont", Email = "alice@example.com" }
        };

        _contactRepositoryMock
            .Setup(x => x.GetAllAsync(search))
            .ReturnsAsync(contacts);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(It.IsAny<Contact>()))
            .Returns((Contact c) => new ContactDto { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Email = c.Email });

        // Act
        var result = await _sut.GetAllAsync(1, 10, search);

        // Assert
        _contactRepositoryMock.Verify(x => x.GetAllAsync(search), Times.Once);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    // ------------------------------------------------------------------ GetByIdAsync

    [Test]
    public async Task GetByIdAsync_WhenContactExists_ShouldReturnContactDto()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var dto = new ContactDto
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(contact);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(contact))
            .Returns(dto);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void GetByIdAsync_WhenContactNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        // Act & Assert
        Assert.That(
            async () => await _sut.GetByIdAsync(999),
            Throws.TypeOf<NotFoundException>());
    }

    // ------------------------------------------------------------------ CreateAsync

    [Test]
    public async Task CreateAsync_WhenValidRequest_ShouldReturnCreatedContactDto()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = "0600000000"
        };

        var contact = new Contact
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com",
            Phone = "0600000000"
        };

        var responseDto = new ContactDto
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.EmailExistsAsync(createDto.Email, null))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map<Contact>(createDto))
            .Returns(contact);

        _contactRepositoryMock
            .Setup(x => x.CreateAsync(contact))
            .ReturnsAsync(contact);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(contact))
            .Returns(responseDto);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void CreateAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.EmailExistsAsync(createDto.Email, null))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.That(
            async () => await _sut.CreateAsync(createDto),
            Throws.TypeOf<ConflictException>());
    }

    // ------------------------------------------------------------------ UpdateAsync

    [Test]
    public async Task UpdateAsync_WhenValidRequest_ShouldReturnUpdatedContactDto()
    {
        // Arrange
        const int id = 1;
        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice Updated",
            LastName = "Dupont",
            Email = "alice.updated@example.com"
        };

        var existingContact = new Contact
        {
            Id = id,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        var responseDto = new ContactDto
        {
            Id = id,
            FirstName = "Alice Updated",
            LastName = "Dupont",
            Email = "alice.updated@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(existingContact);

        _contactRepositoryMock
            .Setup(x => x.EmailExistsAsync(updateDto.Email, id))
            .ReturnsAsync(false);

        _mapperMock
            .Setup(x => x.Map(updateDto, existingContact))
            .Returns(existingContact);

        _contactRepositoryMock
            .Setup(x => x.UpdateAsync(existingContact))
            .ReturnsAsync(existingContact);

        _mapperMock
            .Setup(x => x.Map<ContactDto>(existingContact))
            .Returns(responseDto);

        // Act
        var result = await _sut.UpdateAsync(id, updateDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FirstName, Is.EqualTo("Alice Updated"));
    }

    [Test]
    public void UpdateAsync_WhenContactNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        // Act & Assert
        Assert.That(
            async () => await _sut.UpdateAsync(999, updateDto),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void UpdateAsync_WhenEmailTakenByAnotherContact_ShouldThrowConflictException()
    {
        // Arrange
        const int id = 1;
        var updateDto = new UpdateContactDto
        {
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "taken@example.com"
        };

        var existingContact = new Contact
        {
            Id = id,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(existingContact);

        _contactRepositoryMock
            .Setup(x => x.EmailExistsAsync(updateDto.Email, id))
            .ReturnsAsync(true);

        // Act & Assert
        Assert.That(
            async () => await _sut.UpdateAsync(id, updateDto),
            Throws.TypeOf<ConflictException>());
    }

    // ------------------------------------------------------------------ DeleteAsync

    [Test]
    public async Task DeleteAsync_WhenContactExists_ShouldCallDelete()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Dupont",
            Email = "alice@example.com"
        };

        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(contact);

        _contactRepositoryMock
            .Setup(x => x.DeleteAsync(contact))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _contactRepositoryMock.Verify(x => x.DeleteAsync(contact), Times.Once);
    }

    [Test]
    public void DeleteAsync_WhenContactNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _contactRepositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        // Act & Assert
        Assert.That(
            async () => await _sut.DeleteAsync(999),
            Throws.TypeOf<NotFoundException>());
    }
}
