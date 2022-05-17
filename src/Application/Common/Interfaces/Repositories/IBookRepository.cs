using Application.Common.RequestParameters;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    Task<bool> BookAlreadyExists(string title);
    Task<IList<Book>> GetBooksByQuery(string userId, string searchString, 
        int pageNumber, int pageSize);
}