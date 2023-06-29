using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class BlogRepository : IBlogRepository
{
    private readonly DataContext _context;


    public BlogRepository(DataContext context)
    {
        _context = context;
    }


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task AddBlogAsync(Blog blog)
    {
        await _context.AddAsync(blog);
    }

    public IQueryable<Blog> GetAll()
    {
        return _context.Blogs.AsQueryable();
    }

    public void DeleteBlog(Blog blog)
    {
        _context.Remove(blog);
    }

    public Task<Blog> GetBlogAsync(Guid guid)
    {
        return _context.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == guid);
    }
}