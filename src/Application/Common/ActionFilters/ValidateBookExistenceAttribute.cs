using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateBookExistenceAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateBookExistenceAttribute> _logger;
    private readonly bool _shouldExist;


    public ValidateBookExistenceAttribute(IUserRepository userRepository,
        ILogger<ValidateBookExistenceAttribute> logger,
        bool shouldExist = true)
    {
        _userRepository = userRepository;
        _logger = logger;
        _shouldExist = shouldExist;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string bookTitle;
        
        if (!context.ActionArguments.TryGetValue("bookTitle", out object bookTitleObject))
        {
            var bookDto = (BookInDto)context.ActionArguments.SingleOrDefault(arg => arg.Key.Contains("Dto")).Value;
            if (bookDto == null)
            {
                throw new InternalServerException("Action filter: Expected parameter 'bookTitle' or" +
                                                  "an parameter containing 'Dto' not exist");
            }

            bookTitle = bookDto.Title;
        }
        else
        {
            bookTitle = bookTitleObject.ToString();
        }
        
        
        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        if (user.Books.Any(book => book.Title == bookTitle) != _shouldExist)
        {
            _logger.LogWarning(_shouldExist ? 
                "No book with this title exists" : "A book with this title already exists");

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode,
                _shouldExist ? "No book with this title exists" : "A book with this title already exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }


        await next();
    }
}