using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateTagExistenceAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateTagExistenceAttribute> _logger;
    private readonly bool _shouldExist;


    public ValidateTagExistenceAttribute(IUserRepository userRepository, ILogger<ValidateTagExistenceAttribute> logger,
         bool shouldExist = true)
    {
        _userRepository = userRepository;
        _logger = logger;
        _shouldExist = shouldExist;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string tagName;
        
        if (!context.ActionArguments.TryGetValue("tagName", out object tagNameObject))
        {
            var tagDto = (TagInDto)context.ActionArguments.SingleOrDefault(arg => arg.Key.Contains("Dto")).Value;
            if (tagDto == null)
            {
                throw new InternalServerException("Action filter: Expected parameter 'tagName' or" +
                                                  "an parameter containing 'Dto' not exist");
            }

            tagName = tagDto.Name;
        }
        else
        {
            tagName = tagNameObject.ToString();
        }
        

        var user = await _userRepository.GetAsync(context.HttpContext.User.Identity!.Name, trackChanges: true);
        if (user.Tags.Any(tag => tag.Name == tagName) != _shouldExist)
        {
            _logger.LogWarning(_shouldExist ? 
                "No tag with this name exists" : "A tag with this name already exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                _shouldExist ? "No tag with this name exists" : "A tag with this name already exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
            
        await next();
    }
}