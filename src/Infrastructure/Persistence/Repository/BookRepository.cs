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

    public async Task<bool> BookAlreadyExists(string title)
    {
        return await _context.Books.AnyAsync(book => book.Title == title);
    }

    public async Task<ICollection<Book>> GetBooksByQuery(string userId, BookRequestParameter bookRequestParameter)
    {
        var books = await _context.Books
            .Where(book => book.UserId == userId)
            .Select(book => new
            {
                book,
                orderController = book.Title.StartsWith(bookRequestParameter.Query)
                    ? 1
                    : (book.Title.Contains(bookRequestParameter.Query))
                        ? 2
                        : 3
            })
            .OrderBy(f => f.orderController)
            .ThenBy(f => f.book.Title)
            .Select(f => f.book)
            .Skip((bookRequestParameter.PageNumber - 1) * bookRequestParameter.PageSize)
            .Take(bookRequestParameter.PageSize)
            .ToListAsync();

        return books;
    }
}