using Application.Common.DTOs.Books;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class BookAutoMapperProfile : Profile
{
    public BookAutoMapperProfile()
    {
        CreateMap<BookInDto, Book>()
            .ForMember(dest => dest.DataLink, temp => temp.MapFrom(src => "none"));

        CreateMap<Book, BookOutDto>();
    }
}