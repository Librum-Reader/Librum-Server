using Application.Common.DTOs.Tags;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class TagAutoMapperProfile : Profile
{
    public TagAutoMapperProfile()
    {
        CreateMap<TagInDto, Tag>()
            .ForMember(dest => dest.CreationDate, temp => temp.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.TagId, temp => temp.MapFrom(src => new Guid(src.Guid)));
        CreateMap<Tag, TagOutDto>()
            .ForMember(dest => dest.Guid, temp => temp.MapFrom(src => src.TagId.ToString()));
    }
}