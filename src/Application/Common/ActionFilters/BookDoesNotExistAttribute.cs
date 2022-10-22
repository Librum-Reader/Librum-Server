using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class BookDoesNotExistAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BookDoesNotExistAttribute> _logger;


    public BookDoesNotExistAttribute(IUserRepository userRepository,
                                     ILogger<BookDoesNotExistAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        var bookInDto = (BookInDto)context
            .ActionArguments
            .SingleOrDefault(arg => arg.Key.Contains("Dto"))
            .Value;
        if (bookInDto == null)
        {
            const string message = "Action filter: Expected parameter" +
                                   " containing 'Dto' does not exist";
            throw new InternalServerException(message);
        }

        
        var bookGuid = bookInDto.Guid;

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);
        
        if (user.Books.Any(book => book.BookId.ToString() == bookGuid))
        {
            _logger.LogWarning("A book with this GUID already exists");

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                                               "A book with this GUID already exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }

        await next();
    }
}