using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    public Task LoadRelationShipsAsync(Book book);
    public Task LoadRelationShipsAsync(IEnumerable<Book> books);
    public IQueryable<Book> GetBooks(string userId);
    public Task<Book> GetBook(string userId, string bookTitle, bool trackChances);
    void DeleteBook(Book book);
}