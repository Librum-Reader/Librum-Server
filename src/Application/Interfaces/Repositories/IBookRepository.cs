using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    public Task LoadRelationShipsAsync(Book book);
    public IQueryable<Book> GetAllAsync(string userId, bool loadRelationships = false);
    public Task<bool> ExistsAsync(string userId, Guid bookGuid);
    void DeleteBook(Book book);
    public Task<long> GetUsedBookStorage(string userId);
}