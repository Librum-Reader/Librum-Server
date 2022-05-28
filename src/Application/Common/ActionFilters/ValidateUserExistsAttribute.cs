using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateUserExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateUserExistsAttribute> _logger;


    public ValidateUserExistsAttribute(IUserRepository userRepository, ILogger<ValidateUserExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userEmail = context.HttpContext.User.Identity!.Name;
        if (await _userRepository.GetAsync(userEmail, trackChanges: false) == null)
        {
            _logger.LogWarning("No user with this email exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                "No user with this email exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
        await next();
    }
}