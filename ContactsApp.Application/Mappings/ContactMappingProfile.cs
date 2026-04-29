using AutoMapper;
using ContactsApp.Application.DTOs.Contacts;
using ContactsApp.Domain.Entities;

namespace ContactsApp.Application.Mappings;

public class ContactMappingProfile : Profile
{
    public ContactMappingProfile()
    {
        CreateMap<Contact, ContactDto>();
        CreateMap<CreateContactDto, Contact>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        CreateMap<UpdateContactDto, Contact>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
    }
}
