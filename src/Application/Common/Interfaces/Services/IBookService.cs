using Application.Common.DTOs.Books;
using Application.Common.RequestParameters;

namespace Application.Common.Interfaces.Services;

public interface IBookService
{
    public Task CreateBookAsync(string email, BookInDto bookInDto);
    Task<IList<BookOutDto>> GetBooksAsync(BookRequestParameter bookRequestParameter);
}