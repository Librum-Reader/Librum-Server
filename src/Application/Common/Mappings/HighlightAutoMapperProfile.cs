using Application.Common.DTOs.Highlights;
using Application.Common.DTOs.Tags;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class HighlightAutoMapperProfile : Profile
{
    public HighlightAutoMapperProfile()
    {
        CreateMap<HighlightInDto, Highlight>()
            .ForMember(dest => dest.HighlightId, temp => temp.MapFrom(src => src.Guid));
        CreateMap<Highlight, HighlightOutDto>()
            .ForMember(dest => dest.Guid, temp => temp.MapFrom(src => src.HighlightId.ToString()));
    }
}