using Application.Common.ActionFilters;
using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.RequestParameters;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ServiceFilter(typeof(UserExistsAttribute))]
[ServiceFilter(typeof(ValidParameterAttribute))]
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly ILogger<BookController> _logger;
    private readonly IBookService _bookService;


    public BookController(ILogger<BookController> logger,
                          IBookService bookService)
    {
        _logger = logger;
        _bookService = bookService;
    }


    [HttpPost("create")]
    [TypeFilter(typeof(BookDoesNotExistAttribute))]
    public async Task<ActionResult> CreateBook([FromBody] BookInDto bookInDto)
    {
        if (!bookInDto.IsValid)
        {
            _logger.LogWarning("Creating book failed: data is invalid");
            return BadRequest("The provided data is invalid");
        }

        await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name,
                                           bookInDto, bookInDto.Guid);
        return StatusCode(201);
    }

    [HttpPost("get")]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks(
        [FromBody] BookRequestParameter bookRequestParameter)
    {
        var userName = HttpContext.User.Identity!.Name;
        var books = await _bookService.GetBooksAsync(userName,
                                                     bookRequestParameter);
        return Ok(books);
    }

    [HttpPost("tags/{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    public async Task<ActionResult> AddTagsToBook(
        [FromBody] IEnumerable<string> tagNames, string bookGuid)
    {
        try
        {
            var userName = HttpContext.User.Identity!.Name;
            await _bookService.AddTagsToBookAsync(userName, bookGuid, tagNames);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("tags/{bookGuid}/{tagName}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    [ServiceFilter(typeof(BookHasTagAttribute))]
    public async Task<ActionResult> RemoveTagFromBook(string bookGuid,
                                                      string tagName)
    {
        var userName = HttpContext.User.Identity!.Name;
        await _bookService.RemoveTagFromBookAsync(userName, bookGuid, tagName);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteBooks([FromBody] ICollection<string> bookGuids)
    {
        try
        {
            var userName = HttpContext.User.Identity!.Name;
            await _bookService.DeleteBooksAsync(userName, bookGuids);
            return NoContent();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    public async Task<ActionResult> PatchBook(
        [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc, string bookGuid)
    {
        try
        {
            var userName = HttpContext.User.Identity!.Name;
            await _bookService.PatchBookAsync(userName, patchDoc, bookGuid, this);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("authors/{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    [ServiceFilter(typeof(AuthorDoesNotExistAttribute))]
    public async Task<ActionResult> AddAuthorToBook([FromBody] AuthorInDto authorInDto,
                                                    string bookGuid)
    {
        var userName = HttpContext.User.Identity!.Name;
        await _bookService.AddAuthorToBookAsync(userName, bookGuid, authorInDto);
        return Ok();
    }

    [HttpDelete("authors/{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    [ServiceFilter(typeof(AuthorExistsAttribute))]
    public async Task<ActionResult> RemoveAuthorFromBook(
        [FromBody] AuthorForRemovalDto authorDto, string bookGuid)
    {
        var userName = HttpContext.User.Identity!.Name;
        await _bookService.RemoveAuthorFromBookAsync(userName, bookGuid, authorDto);
        return Ok();
    }
}