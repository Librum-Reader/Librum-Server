using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class ProductRepository(DataContext context) : IProductRepository
{
    public DataContext Context { get; } = context;

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    public void CreateProduct(Product product)
    {
        Context.Add(product);
    }

    public IQueryable<Product> GetAll()
    {
        return Context.Products.Include(p => p.Features);
    }

    public async Task<Product> GetByIdAsync(string id)
    {
        return await Context.Products.Include(p => p.Features).SingleOrDefaultAsync(p => p.ProductId == id);
    }

    public void DeleteProduct(Product product)
    {
        Context.Remove(product);
    }
}