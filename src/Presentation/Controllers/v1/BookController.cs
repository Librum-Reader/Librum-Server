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


    public BookController(ILogger<BookController> logger, IBookService bookService)
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

        await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name, bookInDto, bookInDto.Guid);
        return StatusCode(201);
    }

    [HttpPost("get")]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks([FromBody] BookRequestParameter bookRequestParameter)
    {
        var books = await _bookService.GetBooksAsync(HttpContext.User.Identity!.Name, bookRequestParameter);
        return Ok(books);
    }

    [HttpPost("tags/{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    public async Task<ActionResult> AddTags([FromBody] IEnumerable<string> tagNames, string bookGuid)
    {
        try
        {
            await _bookService.AddTagsToBookAsync(HttpContext.User.Identity!.Name, bookGuid, tagNames);
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
    public async Task<ActionResult> RemoveTagFromBook(string bookGuid, string tagName)
    {
        await _bookService.RemoveTagFromBookAsync(HttpContext.User.Identity!.Name, bookGuid, tagName);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteBooks([FromBody] ICollection<string> bookGuids)
    {
        try
        {
            await _bookService.DeleteBooksAsync(HttpContext.User.Identity!.Name, bookGuids);
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
    public async Task<ActionResult> PatchBook([FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc,
        string bookGuid)
    {
        try
        {
            await _bookService.PatchBookAsync(HttpContext.User.Identity!.Name, patchDoc, bookGuid, this);
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
    public async Task<ActionResult> AddAuthorToBook([FromBody] AuthorInDto authorInDto, string bookGuid)
    {
        await _bookService.AddAuthorToBookAsync(HttpContext.User.Identity!.Name, bookGuid, authorInDto);
        return Ok();
    }

    [HttpDelete("authors/{bookGuid}")]
    [ServiceFilter(typeof(BookExistsAttribute))]
    [ServiceFilter(typeof(AuthorExistsAttribute))]
    public async Task<ActionResult> RemoveAuthorFromBook([FromBody] AuthorForRemovalDto authorDto, string bookGuid)
    {
        await _bookService.RemoveAuthorFromBookAsync(HttpContext.User.Identity!.Name, bookGuid, authorDto);
        return Ok();
    }
}