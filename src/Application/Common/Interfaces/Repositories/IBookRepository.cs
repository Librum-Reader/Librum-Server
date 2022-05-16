using Application.Common.RequestParameters;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    Task<bool> BookAlreadyExists(string title);
    Task<ICollection<Book>> GetBooksByQuery(string userId, BookRequestParameter bookRequestParameter);
}