using Application.Common.DTOs.Books;
using AutoMapper;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Application.Common.Mappings;

public class BookAutoMapperProfile : Profile
{
    public BookAutoMapperProfile()
    {
        CreateMap<BookInDto, Book>()
            .ForMember(dest => dest.DataLink, temp => temp.MapFrom(src => "none"))
            // .ForMember(dest => dest.CoverLink, temp => temp.MapFrom(src => src.Cover.IsNullOrEmpty() 
                                                                        // ? "none"
                                                                        // : src.Cover))
            .ForMember(dest => dest.Tags, temp => temp.Ignore());

        CreateMap<Book, BookOutDto>()
            .ForMember(dest => dest.Format, temp => temp.MapFrom(src => src.Format.ToString()))
            .ForMember(dest => dest.Guid, temp => temp.MapFrom(src => src.BookId.ToString()))
            .ForMember(dest => dest.Cover, temp => temp.MapFrom(src => src.CoverLink));

        CreateMap<Book, BookForUpdateDto>()
            .ForMember(dest => dest.Tags, temp => temp.Ignore());
        
        CreateMap<BookForUpdateDto, Book>();
    }
}