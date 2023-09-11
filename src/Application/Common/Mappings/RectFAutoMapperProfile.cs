using Application.Common.DTOs.Highlights;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class RectFAutoMapperProfile : Profile
{
    public RectFAutoMapperProfile()
    {
        CreateMap<RectFInDto, RectF>();
        CreateMap<RectF, RectFOutDto>();
    }
}