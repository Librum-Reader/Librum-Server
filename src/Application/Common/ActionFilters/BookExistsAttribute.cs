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
            var errorMessage = "No book with this GUID exists";
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