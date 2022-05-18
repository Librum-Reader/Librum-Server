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

    public bool Exists(User user, TagInDto tagIn)
    {
        return user.Tags.Any(tag => tag.Name == tagIn.Name);
    }

    public Tag Get(User user, string name)
    {
        return user.Tags.SingleOrDefault(tag => tag.Name == name);
    }

    public void DeleteTag(Tag tag)
    {
        _context.Remove(tag);
    }
}