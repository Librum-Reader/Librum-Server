using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateBookHasTagAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<ValidateBookHasTagAttribute> _logger;


    public ValidateBookHasTagAttribute(IUserRepository userRepository, IBookRepository bookRepository,
        ILogger<ValidateBookHasTagAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("bookTitle", out object bookTitleObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'bookTitle' does not exist");
        }
        
        if (!context.ActionArguments.TryGetValue("tagName", out object tagNameObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'tagName' does not exist");
        }

        
        var bookTitle = bookTitleObject.ToString();
        var tagName = tagNameObject.ToString();

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        
        var book = user.Books.Single(book => book.Title == bookTitle);
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