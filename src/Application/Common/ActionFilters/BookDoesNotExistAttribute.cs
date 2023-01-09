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
            var errorMessage = "A book with this GUID already exists";
            _logger.LogWarning(errorMessage);

            context.HttpContext.Response.ContentType = "application/json";
            var badRequest = (int)HttpStatusCode.BadRequest;
            context.HttpContext.Response.StatusCode = badRequest;

            var response = new ApiExceptionDto(badRequest, errorMessage);
            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }

        await next();
    }
}