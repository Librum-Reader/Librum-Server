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
[ServiceFilter(typeof(ValidateUserExistsAttribute))]
[ServiceFilter(typeof(ValidateStringParameterAttribute))]
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
    [TypeFilter(typeof(ValidateBookDoesNotExistAttribute))]
    public async Task<ActionResult> CreateBook([FromBody] BookInDto bookInDto)
    {
        if (!bookInDto.IsValid)
        {
            _logger.LogWarning("Creating book failed: data is invalid");
            return BadRequest("The provided data is invalid");
        }

        await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name, bookInDto);
        return StatusCode(201);
    }

    [HttpPost("get")]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks([FromBody] BookRequestParameter bookRequestParameter)
    {
        var books = await _bookService.GetBooksAsync(HttpContext.User.Identity!.Name, bookRequestParameter);
        return Ok(books);
    }

    [HttpPost("tags/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> AddTags([FromBody] IEnumerable<string> tagNames, string bookTitle)
    {
        try
        {
            await _bookService.AddTagsToBookAsync(HttpContext.User.Identity!.Name, bookTitle, tagNames);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("tags/{bookTitle}/{tagName}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    [ServiceFilter(typeof(ValidateBookHasTagAttribute))]
    public async Task<ActionResult> RemoveTagFromBook(string bookTitle, string tagName)
    {
        await _bookService.RemoveTagFromBookAsync(HttpContext.User.Identity!.Name, bookTitle, tagName);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteBooks([FromBody] ICollection<string> bookTitles)
    {
        try
        {
            await _bookService.DeleteBooksAsync(HttpContext.User.Identity!.Name, bookTitles);
            return NoContent();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> PatchBook([FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc,
        string bookTitle)
    {
        try
        {
            await _bookService.PatchBookAsync(HttpContext.User.Identity!.Name, patchDoc, bookTitle, this);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("authors/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    [ServiceFilter(typeof(ValidateAuthorDoesNotExistAttribute))]
    public async Task<ActionResult> AddAuthorToBook([FromBody] AuthorInDto authorInDto, string bookTitle)
    {
        await _bookService.AddAuthorToBookAsync(HttpContext.User.Identity!.Name, bookTitle, authorInDto);
        return Ok();
    }

    [HttpDelete("authors/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    [ServiceFilter(typeof(ValidateAuthorExistsAttribute))]
    public async Task<ActionResult> RemoveAuthorFromBook([FromBody] AuthorForRemovalDto authorDto, string bookTitle)
    {
        await _bookService.RemoveAuthorFromBookAsync(HttpContext.User.Identity!.Name, bookTitle, authorDto);
        return Ok();
    }
}