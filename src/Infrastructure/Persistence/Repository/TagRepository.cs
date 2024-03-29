using Application.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence.Repository;

public class TagRepository : ITagRepository
{
    private readonly DataContext _context;


    public TagRepository(DataContext context)
    {
        _context = context;
    }
    
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Add(Tag tag)
    {
        _context.Add(tag);
    }

    public void Delete(Tag tag)
    {
        _context.Tags.Remove(tag);
    }
}