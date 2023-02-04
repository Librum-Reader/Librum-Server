using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    public Task LoadRelationShipsAsync(Book book);
    public Task LoadRelationShipsAsync(IEnumerable<Book> books);
    public IQueryable<Book> GetAllAsync(string userId);
    public Task<bool> ExistsAsync(string userId, Guid bookGuid);
    void DeleteBook(Book book);
}