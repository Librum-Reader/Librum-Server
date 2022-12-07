using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class TagExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TagExistsAttribute> _logger;


    public TagExistsAttribute(IUserRepository userRepository,
                              ILogger<TagExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {

        if (!context.ActionArguments.TryGetValue("guid",
                                                 out object guidObject))
        {
            const string message = "Action filter: Expected" +
                                   " parameter 'guid'";
            throw new InternalServerException(message);
        }
        
        var guid = guidObject.ToString();
        if (guid == null)
        {
            const string message = "Action filter: Parameter is null";
            throw new InternalServerException(message);
        }

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);

        if (user.Tags.All(tag => tag.TagId != new Guid(guid)))
        {
            _logger.LogWarning("No tag with this guid exists");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
            var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode,
                                               "No tag with this guid exists");

            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }
        
            
        await next();
    }
}