using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateAuthorDoesNotExistAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<ValidateAuthorDoesNotExistAttribute> _logger;


    public ValidateAuthorDoesNotExistAttribute(IUserRepository userRepository, IBookRepository bookRepository
        , ILogger<ValidateAuthorDoesNotExistAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorForRemovalDto = (AuthorInDto)context.ActionArguments
                                                      .SingleOrDefault(arg => arg.Key.Contains("Dto"))
                                                      .Value;
        if (authorForRemovalDto == null)
        {
            throw new InternalServerException("Action filter: Expected parameter containing 'Dto' does not exist");
        }

        if(!context.ActionArguments.TryGetValue("bookTitle", out object bookTitleObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'bookTitle' does not exist");
        }
        
        

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        
        var bookTitle = bookTitleObject.ToString();
        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        await _bookRepository.LoadRelationShipsAsync(book);

        if (book!.Authors.Any(author =>
                author.FirstName == authorForRemovalDto.FirstName && author.LastName == authorForRemovalDto.LastName))
        {
            _logger.LogWarning("An author with this name already exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                "An author with this name already exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
        
        await next();
    }
}