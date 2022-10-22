using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.RequestParameters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces.Services;

public interface IBookService
{
    Task CreateBookAsync(string email, BookInDto bookInDto, string guid);
    Task<IList<BookOutDto>> GetBooksAsync(string email, BookRequestParameter request);
    Task AddTagsToBookAsync(string email, string bookGuid, IEnumerable<string> tagNames);
    Task RemoveTagFromBookAsync(string email, string bookGuid, string tagName);
    Task DeleteBooksAsync(string email, IEnumerable<string> bookGuids);
    Task PatchBookAsync(string email, JsonPatchDocument<BookForUpdateDto> patchDoc, string bookGuid,
        ControllerBase controllerBase);
    
    Task AddAuthorToBookAsync(string email, string bookGuid, AuthorInDto authorToAdd);
    Task RemoveAuthorFromBookAsync(string email, string bookGuid, AuthorForRemovalDto authorToRemove);
}