using Application.Common.DTOs.Books;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IBookRepository
{
    public Task<int> SaveChangesAsync();
    Task<bool> BookAlreadyExists(string title);
}