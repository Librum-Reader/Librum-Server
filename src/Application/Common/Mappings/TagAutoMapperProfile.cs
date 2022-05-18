using Application.Common.DTOs.Tags;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class TagAutoMapperProfile : Profile
{
    public TagAutoMapperProfile()
    {
        CreateMap<TagInDto, Tag>();
        CreateMap<Tag, TagOutDto>();
    }
}