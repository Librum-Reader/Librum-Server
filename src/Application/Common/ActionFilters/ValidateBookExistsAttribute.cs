using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateBookExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateBookExistsAttribute> _logger;


    public ValidateBookExistsAttribute(IUserRepository userRepository, ILogger<ValidateBookExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("bookTitle", out object bookTitleObject))
            throw new InternalServerException("Action filter expected a parameter 'bookTitle' which does not exist");
        

        var bookTitle = bookTitleObject.ToString();

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        if (!user.Books.Any(book => book.Title == bookTitle))
        {
            _logger.LogWarning("No book with this title exists");
            context.Result = new BadRequestResult();
            return;
        }
        
            
        await next();
    }
}