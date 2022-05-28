using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateTagDoesNotExistAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateTagDoesNotExistAttribute> _logger;


    public ValidateTagDoesNotExistAttribute(IUserRepository userRepository, 
        ILogger<ValidateTagDoesNotExistAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tagInDto = (TagInDto)context.ActionArguments.Single(arg => arg.Key.Contains("Dto")).Value;
        if (tagInDto == null)
        {
            throw new InternalServerException("Action filter: Expected parameter containing 'Dto' does not exist");
        }

        var tagName = tagInDto.Name;

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        if (user.Tags.Any(tag => tag.Name == tagName))
        {
            _logger.LogWarning("A tag with this name already exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                "A tag with this name already exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
            
        await next();
    }
}