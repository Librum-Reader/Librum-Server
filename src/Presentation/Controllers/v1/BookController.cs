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


    [HttpPost]
    public async Task<ActionResult> CreateBook([FromBody] BookInDto bookInDto)
    {
        if (bookInDto == null)
        {
            _logger.LogWarning("Creating book failed: book input dto is null");
            return BadRequest("The provided data is invalid");
        }

        try
        {
            for (int i = 900; i < 1000; ++i)
            {
                var bookDto = new BookInDto
                {
                    Pages = bookInDto.Pages,
                    Format = bookInDto.Format,
                    Authors = bookInDto.Authors,
                    CurrentPage = 2,
                    Title = bookInDto.Title + i.ToString()
                };
                await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name, bookDto);
            }

            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Creating book failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
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
}