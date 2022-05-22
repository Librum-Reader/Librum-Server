using Application.Common.DTOs.Books;
using Application.Common.RequestParameters;

namespace Application.Common.Interfaces.Services;

public interface IBookService
{
    Task CreateBookAsync(string email, BookInDto bookInDto);
    Task<IList<BookOutDto>> GetBooksAsync(string email, BookRequestParameter bookRequestParameter);
    Task AddTagsToBookAsync(string email, string bookTitle, IEnumerable<string> tagNames);
    Task RemoveTagFromBookAsync(string email, string bookTitle, string tagName);
    Task DeleteBooksAsync(string email, IEnumerable<string> bookTitles);
}