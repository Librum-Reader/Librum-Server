using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IProductRepository
{
    public Task<int> SaveChangesAsync();
    public void CreateProduct(Product product);
    public IQueryable<Product> GetAll();
    public Task<Product> GetByIdAsync(string id);
    public void DeleteProduct(Product product);
}