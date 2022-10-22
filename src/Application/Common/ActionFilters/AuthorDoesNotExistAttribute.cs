using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class AuthorDoesNotExistAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<AuthorDoesNotExistAttribute> _logger;


    public AuthorDoesNotExistAttribute(IUserRepository userRepository,
                                       IBookRepository bookRepository,
                                       ILogger<AuthorDoesNotExistAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        var authorToRemoveDto = (AuthorInDto)context
            .ActionArguments
            .SingleOrDefault(arg => arg.Key.Contains("Dto"))
            .Value;
        if (authorToRemoveDto == null)
        {
            const string message = "Action filter: Expected parameter " +
                                   "containing 'Dto' does not exist";
            throw new InternalServerException(message);
        }

        if(!context.ActionArguments.TryGetValue("bookGuid",
                                                out object bookGuidObject))
        {
            const string message = "Action filter: Expected parameter" +
                                   " 'bookGuid' does not exist";
            throw new InternalServerException(message);
        }

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);
        
        var bookGuid = bookGuidObject.ToString();
        var book = user.Books.SingleOrDefault(book => book.BookId.ToString() == bookGuid);
        await _bookRepository.LoadRelationShipsAsync(book);

        if (book!.Authors.Any(author =>
                author.FirstName == authorToRemoveDto.FirstName &&
                author.LastName == authorToRemoveDto.LastName))
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