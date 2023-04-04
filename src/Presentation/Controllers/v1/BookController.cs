using Application.Common.Attributes;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

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

    /// Books are created in two steps, first of all an entry containing all of their
    /// meta data is created on the Database (See 'CreateBook()') and then their
    /// binary data, so their actual content is added to the existing book.
    [HttpPost("addBookBinaryData/{guid:guid}")]
    [DisableFormValueModelBinding]
    public async Task<ActionResult> AddBookBinaryData(Guid guid)
    {
        // Check if the book binary data was sent in the correct format
        var isMultiPart = !string.IsNullOrEmpty(Request.ContentType) &&
                          Request.ContentType.IndexOf(
                              "multipart/",
                              StringComparison.OrdinalIgnoreCase) >= 0;
        if (!isMultiPart)
        {
            var message = "The book binary data needs to be sent as multipart";
            return BadRequest(message);
        }
        
        var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        try
        {
            await _bookService.AddBookBinaryData(HttpContext.User.Identity!.Name,
                                                 guid, reader);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }

        return Ok();
    }

    [HttpGet("getBookBinaryData/{guid:guid}")]
    public async Task<ActionResult> GetBookBinaryData(Guid guid)
    {
        Stream stream;
        try
        {
            stream = await _bookService.GetBookBinaryData(HttpContext.User.Identity!.Name, 
                                                          guid);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }

        return File(stream, "application/octet-stream");
    }
    
    private static string GetBoundary(MediaTypeHeaderValue contentType)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        return boundary;
    }

    [HttpPost]
    public async Task<ActionResult> CreateBook(BookInDto bookInDto)
    {
        if (!bookInDto.IsValid)
        {
            _logger.LogWarning("Creating book failed: data is invalid");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name,
                                               bookInDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks()
    {
        var userName = HttpContext.User.Identity!.Name;
        var books = await _bookService.GetBooksAsync(userName);

        return Ok(books);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteBooks(ICollection<Guid> bookGuids)
    {
        if (bookGuids.IsNullOrEmpty())
        {
            _logger.LogWarning("Deleting book failed: no book guids provided");
            return BadRequest("No books provided");
        }

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

    [HttpPut]
    public async Task<ActionResult> UpdateBook([FromBody] BookForUpdateDto bookDto)
    {
        try
        {
            var userName = HttpContext.User.Identity!.Name;
            await _bookService.UpdateBookAsync(userName, bookDto);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}