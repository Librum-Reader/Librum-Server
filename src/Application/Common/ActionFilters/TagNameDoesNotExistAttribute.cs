using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class TagNameDoesNotExistAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TagNameDoesNotExistAttribute> _logger;


    public TagNameDoesNotExistAttribute(IUserRepository userRepository,
                                        ILogger<TagNameDoesNotExistAttribute>
                                            logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        var tagInDto = (TagInDto)context.ActionArguments.SingleOrDefault(
            arg => arg.Key.Contains("Dto")).Value;
        if (tagInDto == null)
        {
            const string message = "Action filter: Expected " +
                                   "parameter containing 'Dto'";
            throw new InternalServerException(message);
        }

        var tagName = tagInDto.Name;

        var userName = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(userName, trackChanges: true);

        if (user.Tags.Any(tag => tag.Name == tagName))
        {
            var errorMessage = "A tag with this name already exists";
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