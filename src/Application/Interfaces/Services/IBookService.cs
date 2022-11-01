using Application.Common.DTOs.Books;

namespace Application.Interfaces.Services;

public interface IBookService
{
    Task CreateBookAsync(string email, BookInDto bookInDto, string guid);
    Task<IList<BookOutDto>> GetBooksAsync(string email);
    Task AddTagsToBookAsync(string email, string bookGuid,
                            IEnumerable<string> tagNames);
    Task RemoveTagFromBookAsync(string email, string bookGuid, string tagName);
    Task DeleteBooksAsync(string email, IEnumerable<string> bookGuids);
    Task PatchBookAsync(string email, BookForUpdateDto bookUpdateDto,
                        string bookGuid);
}