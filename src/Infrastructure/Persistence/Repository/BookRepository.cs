using Application.Common.Interfaces.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence.Repository;

public class BookRepository : IBookRepository
{
    private readonly DataContext _context;


    public BookRepository(DataContext context)
    {
        _context = context;
    }


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task LoadRelationShipsAsync(Book book)
    {
        await _context.Entry(book).Collection(p => p.Authors).LoadAsync();
        await _context.Entry(book).Collection(p => p.Tags).LoadAsync();
    }

    public async Task LoadRelationShipsAsync(IEnumerable<Book> books)
    {
        foreach (var book in books)
        {
            await LoadRelationShipsAsync(book);
        }
    }

    public IQueryable<Book> GetBooks(string userId)
    {
        return _context.Books.Where(book => book.UserId == userId);
    }
}