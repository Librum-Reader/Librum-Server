using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        await _context.Entry(book).Collection(p => p.Tags).LoadAsync();
    }

    public async Task LoadRelationShipsAsync(IEnumerable<Book> books)
    {
        foreach (var book in books)
        {
            await LoadRelationShipsAsync(book);
        }
    }

    public IQueryable<Book> GetAllAsync(string userId)
    {
        return _context.Books.Where(book => book.UserId == userId);
    }

    public async Task<bool> ExistsAsync(string userId, string bookTitle)
    {
        return await _context.Books.AnyAsync(book => book.UserId == userId &&
                                                     book.Title == bookTitle);
    }

    public void DeleteBook(Book book)
    {
        _context.Remove(book);
    }
}