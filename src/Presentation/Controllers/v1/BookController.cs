using Application.Common.Attributes;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Azure.Storage.Blobs;
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
    private readonly BlobServiceClient _blobServiceClient;

    
    public BookController(ILogger<BookController> logger, IBookService bookService,
                          BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _bookService = bookService;
        _blobServiceClient = blobServiceClient;
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
        
        // await _bookService.AddBookBinaryData(HttpContext.User.Identity!.Name, 
        //                                      guid,
        //                                      reader);
        
        var containerClient =
            _blobServiceClient.GetBlobContainerClient("librumdev");
        var blobClient = containerClient.GetBlobClient(guid.ToString());

        await using var dest = await blobClient.OpenWriteAsync(true);

        
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out var contentDisposition);
        
            if (!hasContentDispositionHeader)
                continue;
        
            if (!HasFileContentDisposition(contentDisposition))
            {
                var message = "Missing content disposition header";
                throw new InvalidParameterException(message);
            }
            
            await section.Body.CopyToAsync(dest);
            
            section = await reader.ReadNextSectionAsync();
        }
        
        return Ok();
    }
    
    private static bool HasFileContentDisposition(
        ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
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