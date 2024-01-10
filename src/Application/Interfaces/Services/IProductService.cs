using Application.Common.DTOs.Product;

namespace Application.Interfaces.Services;

public interface IProductService
{
    public Task<IEnumerable<ProductOutDto>> GetAllProductsAsync();
    public Task CreateProductAsync(ProductInDto productInDto);
    public Task UpdateProductAsync(ProductForUpdateDto productInDto);
    public Task DeleteProductAsync(string id);
    public Task AddPriceToProductAsync(string id, int price);
}