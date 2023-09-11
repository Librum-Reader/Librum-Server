using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class HighlightRepository : IHighlightRepository
{
    private readonly DataContext _context;


    public HighlightRepository(DataContext context)
    {
        _context = context;
    }
    
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Add(Highlight highlight)
    {
        _context.Highlights.Add(highlight);
    }

    public void Delete(Highlight highlight)
    {
        _context.Highlights.Remove(highlight);
    }

    public async Task<Highlight> GetAsync(Guid bookId, Guid highlightGuid)
    {
        return await _context.Highlights.SingleOrDefaultAsync(highlight => 
            highlight.BookId == bookId && 
            highlight.HighlightId == highlightGuid);
    }
}