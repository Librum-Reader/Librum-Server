using Application.Common.DTOs.Books;

namespace Application.Interfaces.Services;

public interface IBookService
{
    Task CreateBookAsync(string email, BookInDto bookInDto, string guid);
    Task<IList<BookOutDto>> GetBooksAsync(string email);
    Task DeleteBooksAsync(string email, IEnumerable<string> bookGuids);
    Task PatchBookAsync(string email, BookForUpdateDto bookUpdateDto,
                        string bookGuid);
}