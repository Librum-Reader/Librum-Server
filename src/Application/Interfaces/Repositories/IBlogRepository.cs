using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IBlogRepository
{
    public Task<int> SaveChangesAsync();
    public Task AddBlogAsync(Blog blog);
    public IQueryable<Blog> GetAll();
    public void DeleteBlog(Blog blog);
    public Task<Blog> GetBlogAsync(Guid guid);
}