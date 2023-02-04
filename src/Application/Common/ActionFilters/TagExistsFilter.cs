using System.Net;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class TagExistsFilter : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TagExistsFilter> _logger;


    public TagExistsFilter(IUserRepository userRepository,
                              ILogger<TagExistsFilter> logger)
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
            var errorMessage = "No tag with this guid exists";
            _logger.LogWarning(errorMessage);

            context.HttpContext.Response.ContentType = "application/json";
            var badRequest = (int)HttpStatusCode.BadRequest;
            context.HttpContext.Response.StatusCode = badRequest;

            var response = new ApiExceptionDto(badRequest, errorMessage);
            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }


        await next();
    }
}