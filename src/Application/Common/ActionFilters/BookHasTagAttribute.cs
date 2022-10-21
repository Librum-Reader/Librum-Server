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


    public BookHasTagAttribute(IUserRepository userRepository, IBookRepository bookRepository,
        ILogger<BookHasTagAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("bookGuid", out object bookGuidObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'bookGuid' does not exist");
        }
        
        if (!context.ActionArguments.TryGetValue("tagName", out object tagNameObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'tagName' does not exist");
        }

        
        var bookGuid = bookGuidObject.ToString();
        var tagName = tagNameObject.ToString();

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        
        var book = user.Books.Single(book => book.BookId.ToString() == bookGuid);
        await _bookRepository.LoadRelationShipsAsync(book);

        if (!book.Tags.Any(tag => tag.Name == tagName))
        {
            _logger.LogWarning("No tag with this name exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                "No tag with this name exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
            
        await next();
    }
}