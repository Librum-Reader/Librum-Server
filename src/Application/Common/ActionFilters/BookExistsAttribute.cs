using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class BookExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BookExistsAttribute> _logger;


    public BookExistsAttribute(IUserRepository userRepository,
                               ILogger<BookExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("bookGuid",
                                                 out object bookGuidObject))
        {
            const string message = "Action filter: Expected parameter" +
                                   " 'bookGuid' does not exist";
            throw new InternalServerException(message);
        }
        
        var bookGuid = bookGuidObject.ToString();

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);
        
        if (user.Books.All(book => book.BookId.ToString() != bookGuid))
        {
            _logger.LogWarning("No book with this GUID exists");

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                                               "No book with this GUID exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }


        await next();
    }
}