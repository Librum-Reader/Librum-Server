using Application.Common.DTOs.Users;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Mappings;

public class UserAutoMapperProfile : Profile
{
    public UserAutoMapperProfile()
    {
        CreateMap<User, UserOutDto>();
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.UserName, temp => temp.MapFrom(src => src.Email))
            .ForMember(dest => dest.AccountCreation,
                       temp => temp.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PasswordHash, temp => temp.Ignore())
            .ForMember(dest => dest.Role, temp => temp.MapFrom(src => "Basic")); // Default role
        CreateMap<User, UserForUpdateDto>()
            .ReverseMap();
    }
}