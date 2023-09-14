using Application.Common.DTOs.Books;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class BookAutoMapperProfile : Profile
{
    public BookAutoMapperProfile()
    {
        CreateMap<BookInDto, Book>()
            .ForMember(dest => dest.Tags, temp => temp.Ignore())
            .ForMember(dest => dest.Highlights, temp => temp.Ignore());

        CreateMap<Book, BookOutDto>()
            .ForMember(dest => dest.Format,
                       temp => temp.MapFrom(src => src.Format.ToString()))
            .ForMember(dest => dest.Guid,
                       temp => temp.MapFrom(src => src.BookId.ToString()));

        CreateMap<Book, BookForUpdateDto>()
            .ForMember(dest => dest.Tags, temp => temp.Ignore());
        
        CreateMap<BookForUpdateDto, Book>();
    }
}