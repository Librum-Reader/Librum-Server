using Application.Common.DTOs.Highlights;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class RectFAutoMapperProfile : Profile
{
    public RectFAutoMapperProfile()
    {
        CreateMap<RectFInDto, RectF>()
            .ForMember(dest => dest.RectFId, temp => temp.MapFrom(src => src.Guid));
        CreateMap<RectF, RectFOutDto>().ForMember(dest => dest.Guid,
                                                  temp => temp.MapFrom(
                                                      src => src.RectFId.ToString()));
    }
}