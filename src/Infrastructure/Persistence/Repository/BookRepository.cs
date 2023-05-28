using System.Diagnostics;
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

    public async Task<bool> ExistsAsync(string userId, Guid bookGuid)
    {
        return await _context.Books.AnyAsync(book => book.UserId == userId &&
                                                     book.BookId == bookGuid);
    }

    public void DeleteBook(Book book)
    {
        _context.Remove(book);
    }

    public async Task<long> GetUsedBookStorage(string userId)
    {
        var coverStorage = await _context.Books.Where(book => book.UserId == userId).SumAsync(book => book.CoverSize);
        var books = await _context.Books.Where(book => book.UserId == userId).ToListAsync();
        var bookStorage = books.Sum(book => GetBytesFromSizeString(book.DocumentSize));

        return coverStorage + (long)bookStorage;
    }

    private double GetBytesFromSizeString(string size)
    {
        size = size.Replace(" ", string.Empty);
        size = size.Replace(",", ".");
        
        int typeBegining = -1;
        for (int i = 0; i < size.Length; i++)
        {
            if (!char.IsDigit(size[i]) && size[i] != '.')
            {
                typeBegining = i;
                break;
            }
        }

        var numberString = size.Substring(0, typeBegining);
        var numbers = double.Parse(numberString);
        var type = size[typeBegining..];

        return type.ToLower() switch
        {
            "b" => numbers,
            "kib" => numbers * 1024,
            "mib" => numbers * 1024 * 1024
        };
    }
}