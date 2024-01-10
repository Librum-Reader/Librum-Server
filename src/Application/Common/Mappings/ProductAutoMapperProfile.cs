using Application.Common.DTOs.Highlights;
using Application.Common.DTOs.Product;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class ProductAutoMapperProfile : Profile
{
    public ProductAutoMapperProfile()
    {
        CreateMap<ProductInDto, Product>()
            .ForMember(dest => dest.ProductId, temp => temp.MapFrom(src => src.Id))
            .ForMember(dest => dest.Features, temp => temp.Ignore())
            .ForMember(dest => dest.Price, temp => temp.Ignore());
        CreateMap<Product, ProductOutDto>()
            .ForMember(dest => dest.Id, temp => temp.MapFrom(src => src.ProductId.ToString()))
            .ForMember(dest => dest.Features, temp => temp.MapFrom(src => src.Features.Select(feature => feature.Name)));
    }
}