using Application.Common.DTOs;
using Application.Common.DTOs.User;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class UserAutoMapperProfile : Profile
{
    public UserAutoMapperProfile()
    {
        CreateMap<User, UserOutDto>();
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.UserName, temp => temp.MapFrom(src => src.Email))
            .ForMember(dest => dest.AccountCreation, temp => temp.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.PasswordHash, temp => temp.Ignore());
        CreateMap<User, UserForUpdateDto>()
            .ReverseMap();
    }
}