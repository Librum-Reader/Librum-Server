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
    public async Task<ActionResult> CreateBook([FromBody] BookInDto bookInDto)
    {
        if (bookInDto == null || !bookInDto.IsValid)
        {
            _logger.LogWarning("Creating book failed: data is invalid");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name, bookInDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Creating book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("get")]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks([FromBody] BookRequestParameter bookRequestParameter)
    {
        if (bookRequestParameter == null)
        {
            _logger.LogWarning("Getting books failed: book request parameter is null");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            var books = await _bookService.GetBooksAsync(HttpContext.User.Identity!.Name, bookRequestParameter);
            return Ok(books);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Getting books failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("tags/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> AddTags([FromBody] IEnumerable<string> tagNames, string bookTitle)
    {
        if (tagNames == null || string.IsNullOrEmpty(bookTitle))
        {
            _logger.LogWarning("Adding tags to book failed: Invalid data");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.AddTagsToBookAsync(HttpContext.User.Identity!.Name, bookTitle, tagNames);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Adding tags to book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete("tags/{bookTitle}/{tagName}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> RemoveTagFromBook(string bookTitle, string tagName)
    {
        if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(bookTitle))
        {
            _logger.LogWarning("Removing tags from book failed: Invalid data");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.RemoveTagFromBookAsync(HttpContext.User.Identity!.Name, bookTitle, tagName);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Removing tags from book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete]
    public async Task<ActionResult> DeleteBooks([FromBody] ICollection<string> bookTitles)
    {
        if (bookTitles.Count == 0)
        {
            _logger.LogWarning("Deleting books failed: the book list is null");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.DeleteBooksAsync(HttpContext.User.Identity!.Name, bookTitles);
            return NoContent();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Deleting books failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> PatchBookAsync([FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc, 
        string bookTitle)
    {
        try
        {
            await _bookService.PatchBookAsync(HttpContext.User.Identity!.Name, patchDoc, bookTitle, this);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Patching book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("author/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> AddAuthorToBook([FromBody] AuthorInDto authorInDto, string bookTitle)
    {
        if (authorInDto == null)
        {
            _logger.LogWarning("Adding author to book failed: the author dto is null");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.AddAuthorToBookAsync(HttpContext.User.Identity!.Name, bookTitle, authorInDto);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Adding author to book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete("author/{bookTitle}")]
    [ServiceFilter(typeof(ValidateBookExistsAttribute))]
    public async Task<ActionResult> RemoveAuthorFromBook([FromBody] AuthorForRemovalDto author, string bookTitle)
    {
        if (author == null)
        {
            _logger.LogWarning("Removing author from book failed: the author dto is null");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.RemoveAuthorFromBookAsync(HttpContext.User.Identity!.Name, bookTitle, author);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Removing author from book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}