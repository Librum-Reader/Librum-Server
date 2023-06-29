using Application.Common.DTOs.Blogs;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class BlogAutoMapperProfile : Profile
{
    public BlogAutoMapperProfile()
    {
        CreateMap<BlogInDto, Blog>()
            .ForMember(dest => dest.CreationDate, temp => temp.MapFrom(src => DateTime.UtcNow));

        CreateMap<Blog, BlogOutDto>()
            .ForMember(dest => dest.Guid, temp => temp.MapFrom(src => src.BlogId));
        
        CreateMap<BlogForUpdateDto, Blog>();
    }
}