using System.Collections.Specialized;
using Application.Common.DTOs.Tags;
using Application.Common.Interfaces.Repositories;
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

    public void Delete(Tag tag)
    {
        _context.Remove(tag);
    }
}