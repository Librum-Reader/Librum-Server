using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
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
            throw new InvalidParameterException("The provided data is invalid");
        }

        try
        {
            await _bookService.CreateBookAsync(HttpContext.User.Identity!.Name, bookInDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Creating book failed: {ErrorMessage}", e.Message);
            throw new InvalidParameterException(e.Message);
        }
    }
}