using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class AuthorExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<AuthorExistsAttribute> _logger;


    public AuthorExistsAttribute(IUserRepository userRepository,
                                 IBookRepository bookRepository,
                                 ILogger<AuthorExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        
        var authorForRemovalDto = (AuthorForRemovalDto)context
            .ActionArguments
            .SingleOrDefault(arg => arg.Key.Contains("Dto"))
            .Value;
        if (authorForRemovalDto == null)
        {
            const string message = "Action filter: Expected parameter " +
                                   "containing 'Dto' does not exist";
            throw new InternalServerException(message);
        }

        if(!context.ActionArguments.TryGetValue("bookGuid",
                                                out object bookGuidObject))
        {
            const string message = "Action filter: Expected parameter " +
                                   "'bookGuid' does not exist";
            throw new InternalServerException(message);
        }

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);
        
        var bookGuid = bookGuidObject.ToString();
        var book = user.Books.SingleOrDefault(book => book.BookId
                                                  .ToString() == bookGuid);
        await _bookRepository.LoadRelationShipsAsync(book);

        if (!book!.Authors.Any(author =>
                author.FirstName == authorForRemovalDto.FirstName &&
                author.LastName == authorForRemovalDto.LastName))
        {
            _logger.LogWarning("No author with this name exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                                               "No author with this name exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
        await next();
    }
}