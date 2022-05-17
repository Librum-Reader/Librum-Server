using Application.Common.RequestParameters;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    Task<bool> BookAlreadyExists(string title);
    public Task LoadRelationShipsAsync(Book book);
    public Task LoadRelationShipsAsync(IEnumerable<Book> books);
    public IQueryable<Book> GetBooks(string userId);
}