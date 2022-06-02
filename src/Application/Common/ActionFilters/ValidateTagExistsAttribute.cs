using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateTagExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateTagExistsAttribute> _logger;


    public ValidateTagExistsAttribute(IUserRepository userRepository, ILogger<ValidateTagExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        if (!context.ActionArguments.TryGetValue("tagName", out object tagNameObject))
        {
            throw new InternalServerException("Action filter: Expected parameter 'tagName'");
        }
        
        var tagName = tagNameObject.ToString();


        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        if (!user.Tags.Any(tag => tag.Name == tagName))
        {
            _logger.LogWarning("No tag with this name exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, "No tag with this name exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
            
        await next();
    }
}