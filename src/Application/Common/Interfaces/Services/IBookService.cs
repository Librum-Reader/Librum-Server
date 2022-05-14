using Application.Common.DTOs.Books;

namespace Application.Common.Interfaces.Services;

public interface IBookService
{
    public Task CreateBook(string email, BookInDto bookInDto);
}