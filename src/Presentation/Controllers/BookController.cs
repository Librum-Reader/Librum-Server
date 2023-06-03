using Application.Common.Attributes;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Presentation.Controllers;

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

    /// Books are created in two steps, first of all an entry containing all of their
    /// meta data is created on the Database (See 'CreateBook()') and then their
    /// binary data, so their actual content is added to the existing book.
    [HttpPost("bookData/{guid:guid}")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(209715200)]   // Allow max 200MB
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
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }

        return Ok();
    }

    [HttpGet("bookData/{guid:guid}")]
    public async Task<ActionResult> GetBookBinaryData(Guid guid)
    {
        try
        {
            var email = HttpContext.User.Identity!.Name;
            var stream = await _bookService.GetBookBinaryData(email, 
                                                          guid);
            
            Response.Headers.Add("Guid", guid.ToString());
            Response.Headers.Add("Format",
                                 await _bookService.GetFormatForBook(email, guid));
            return File(stream, "application/octet-stream");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpPost("cover/{guid:guid}")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(5242880)]   // Allow max 5MB
    public async Task<ActionResult> ChangeCover(Guid guid)
    {
        // Check if the cover was sent in the correct format
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
            await _bookService.ChangeBookCover(HttpContext.User.Identity!.Name,
                                                 guid, reader);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }

        return Ok();
    }

    [HttpGet("cover/{guid:guid}")]
    public async Task<ActionResult> GetCover(Guid guid)
    {
        try
        {
            var stream = await _bookService.GetBookCover(HttpContext.User.Identity!.Name, 
                guid);
            
            Response.Headers.Add("Guid", guid.ToString());
            return File(stream, "application/octet-stream");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpDelete("cover/{guid:guid}")]
    public async Task<ActionResult> DeleteCover(Guid guid)
    {
        try
        {
            await _bookService.DeleteBookCover(HttpContext.User.Identity!.Name, guid);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }

        return Ok();
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
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
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
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
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
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
}