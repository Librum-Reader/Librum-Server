using Application.Common.DTOs.Users;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class UserAutoMapperProfile : Profile
{
    public UserAutoMapperProfile()
    {
        CreateMap<User, UserOutDto>()
            .ForMember(dest => dest.Role, temp => temp.Ignore());
        
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.UserName, temp => temp.MapFrom(src => src.Email))
            .ForMember(dest => dest.AccountCreation,
                temp => temp.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PasswordHash, temp => temp.Ignore())
            .ForMember(dest => dest.ProductId, temp => temp.Ignore());
        
        CreateMap<User, UserForUpdateDto>()
            .ReverseMap();
    }
}