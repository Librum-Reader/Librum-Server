using Application.Common.DTOs.Product;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class ProductService(IMapper mapper, IProductRepository productRepository) : IProductService
{
    private IMapper Mapper { get; } = mapper;
    private IProductRepository ProductRepository { get; } = productRepository;

    public async Task<IEnumerable<ProductOutDto>> GetAllProductsAsync()
    {
        var products = await productRepository.GetAll().ToListAsync();
        return products.Select(product => Mapper.Map<ProductOutDto>(product));
    }

    public async Task CreateProductAsync(ProductInDto productInDto)
    {
        var product = Mapper.Map<Product>(productInDto);
        foreach(var feature in productInDto.Features)
        {
            product.Features.Add(new ProductFeature
            {
                Name = feature
            });
        }
        productRepository.CreateProduct(product);

        await productRepository.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(ProductForUpdateDto productUpdateDto)
    {
        var product = await productRepository.GetByIdAsync(productUpdateDto.Id);
        
        // The update gets sent at the same time as the product is created, we need to wait for the product to be created
        int tries = 0;
        while(product == null && tries < 3)
        {
            await Task.Delay(200);
            product = await productRepository.GetByIdAsync(productUpdateDto.Id);
            tries++;
        }
        
        if (product == null)
        {
            const string message = "No product with this id exists";
            throw new CommonErrorException(404, message, 0);
        }
        
        bool hasChanged = false;
        var dtoProperties = productUpdateDto.GetType().GetProperties();
        foreach (var dtoProperty in dtoProperties)
        {
            // Manually handle certain properties
            switch (dtoProperty.Name)
            {
                case "Id":
                    continue;     // Can't modify the GUID
                case "Features":
                {
                    // Remove all existing features
                    product.Features.Clear();
                    
                    // Add all new features
                    foreach (var feature in productUpdateDto.Features)
                    {
                        product.Features.Add(new ProductFeature
                        {
                            Name = feature
                        });
                    }

                    hasChanged = true;
                    continue;
                }
            }
            
            var productProperty = product.GetType().GetProperty(dtoProperty.Name);
            var productValue = productProperty.GetValue(product);
            var dtoValue = dtoProperty.GetValue(productUpdateDto);
            if (productValue == dtoValue)
                continue;

            // Handle this after the check if the values are the same
            if (dtoProperty.Name == "Price")
            {
                if (productUpdateDto.Price == 0)
                    continue;

                product.Price = productUpdateDto.Price;
            }

            // Update any other property via reflection
            var value = dtoProperty.GetValue(productUpdateDto);
            SetPropertyOnProduct(product, dtoProperty.Name, value);
            hasChanged = true;
        }
        
        if(hasChanged)
            await ProductRepository.SaveChangesAsync();
    }
    
    private void SetPropertyOnProduct(Product product, string property, object value)
    {
        var bookProperty = product.GetType().GetProperty(property);
        if (bookProperty == null)
        {
            var message = "Product has no property called: " + property;
            throw new CommonErrorException(400, message, 0);
        }
        
        bookProperty.SetValue(product, value);
    }

    public async Task DeleteProductAsync(string id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null)
        {
            const string message = "No product with this id exists";
            throw new CommonErrorException(404, message, 0);
        }
        
        productRepository.DeleteProduct(product);
        await productRepository.SaveChangesAsync();
    }

    public async Task AddPriceToProductAsync(string id, string priceId, int price)
    {
        var product = await productRepository.GetByIdAsync(id);
        
        // The price gets sent at the same time as the product is created, we need to wait for the product to be created
        int tries = 0;
        while(product == null && tries < 3)
        {
            await Task.Delay(300);
            product = await productRepository.GetByIdAsync(id);
            tries++;
        }
        
        if (product == null)
        {
            const string message = "No product with this id exists";
            throw new CommonErrorException(404, message, 0);
        }
        
        product.Price = price;
        product.PriceId = priceId;
        await productRepository.SaveChangesAsync();
    }
}