using Application.Common.DTOs.Bookmarks;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class BookmarkAutoMapperProfile : Profile
{
    public BookmarkAutoMapperProfile()
    {
        CreateMap<BookmarkInDto, Bookmark>()
            .ForMember(dest => dest.BookmarkId, temp => temp.MapFrom(src => src.Guid));
        CreateMap<Bookmark, BookmarkOutDto>()
            .ForMember(dest => dest.Guid, temp => temp.MapFrom(src => src.BookmarkId.ToString()));
    }
}