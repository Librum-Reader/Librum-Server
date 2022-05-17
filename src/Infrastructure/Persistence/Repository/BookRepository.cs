using System.Diagnostics;
using Application.Common.Interfaces.Repositories;
using Application.Common.RequestParameters;
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
        await _context.Entry(book).Collection(p => p.Authors).LoadAsync();
    }

    public async Task LoadRelationShipsAsync(IEnumerable<Book> books)
    {
        foreach (var book in books)
        {
            await LoadRelationShipsAsync(book);
        }
    }
    
    public async Task<bool> BookAlreadyExists(string title)
    {
        return await _context.Books.AnyAsync(book => book.Title == title);
    }

    public async Task<IList<Book>> GetBooksByQuery(string userId, string searchString, int pageNumber, int pageSize)
    {
        var books = await _context.Books
            .Where(book => book.UserId == userId)
            .Select(book => new
            {
                book,
                orderController = book.Title.StartsWith(searchString)
                    ? 1
                    : (book.Title.Contains(searchString))
                        ? 2
                        : 3
            })
            .OrderBy(f => f.orderController)
            .ThenBy(f => f.book.Title)
            .Select(f => f.book)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return books;
    }
}