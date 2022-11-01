using Application.Common.ActionFilters;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("get")]
    public async Task<ActionResult<IList<BookOutDto>>> GetBooks()
    {
        var userName = HttpContext.User.Identity!.Name;
        var books = await _bookService.GetBooksAsync(userName);
        
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
    public async Task<ActionResult> PatchBook([FromBody]BookForUpdateDto bookDto,
                                              string bookGuid)
    {
        try
        {
            var userName = HttpContext.User.Identity!.Name;
            await _bookService.PatchBookAsync(userName, bookDto, bookGuid);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}