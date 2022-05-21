using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.RequestParameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ApiController]
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
        if (bookInDto == null)
        {
            _logger.LogWarning("Creating book failed: book input dto is null");
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
            _logger.LogWarning("Creating book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("tags/{bookTitle}")]
    public async Task<ActionResult> AddTag([FromBody] IEnumerable<string> tagNames, string bookTitle)
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
    public async Task<ActionResult> AddTag(string bookTitle, string tagName)
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
}