using Application.Common.DTOs.Authors;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class AuthorAutoMappingProfile : Profile
{
    public AuthorAutoMappingProfile()
    {
        CreateMap<AuthorInDto, Author>();
        CreateMap<Author, AuthorOutDto>();
        CreateMap<AuthorForRemovalDto, Author>();
    }
}