using Application.Common.DTOs.Books;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class BookAutoMapperProfile : Profile
{
    public BookAutoMapperProfile()
    {
        CreateMap<BookInDto, Book>()
            .ForMember(dest => dest.DataLink, temp => temp.MapFrom(src => "none"))
            .ForMember(dest => dest.LastOpened, temp => temp.MapFrom(src => DateTime.Now));

        CreateMap<Book, BookOutDto>();
    }
}