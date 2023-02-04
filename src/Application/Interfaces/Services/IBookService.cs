using Application.Common.DTOs.Books;

namespace Application.Interfaces.Services;

public interface IBookService
{
    Task CreateBookAsync(string email, BookInDto bookInDto);
    Task<IList<BookOutDto>> GetBooksAsync(string email);
    Task DeleteBooksAsync(string email, IEnumerable<Guid> guids);
    Task UpdateBookAsync(string email, BookForUpdateDto bookUpdateDto);
}