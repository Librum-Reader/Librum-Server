using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class BookHasTagAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BookHasTagAttribute> _logger;


    public BookHasTagAttribute(IUserRepository userRepository,
                               IBookRepository bookRepository,
                               ILogger<BookHasTagAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
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

        if (!context.ActionArguments.TryGetValue("tagGuid",
                                                 out object tagGuidObject))
        {
            const string message = "Action filter: Expected parameter" +
                                   " 'tagGuid' does not exist";
            throw new InternalServerException(message);
        }

        var bookGuid = bookGuidObject.ToString();
        var tagGuid = tagGuidObject.ToString();
        if (tagGuid == null || bookGuid == null)
        {
            const string message = "Action filter: Parameter is null";
            throw new InternalServerException(message);
        }

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);

        var book = user.Books.Single(book => book.BookId == new Guid(bookGuid));
        await _bookRepository.LoadRelationShipsAsync(book);

        if (book.Tags.All(tag => tag.TagId != new Guid(tagGuid)))
        {
            var errorMessage = "No tag with this guid exists";
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