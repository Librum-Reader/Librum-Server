using System.Net;
using Application.Common.DTOs;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class UserExistsAttribute : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserExistsAttribute> _logger;


    public UserExistsAttribute(IUserRepository userRepository,
                               ILogger<UserExistsAttribute> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        var email = context.HttpContext.User.Identity!.Name;
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        if (user == null)
        {
            var errorMessage = "No user with this email exists";
            _logger.LogWarning(errorMessage);

            context.HttpContext.Response.ContentType = "application/json";
            var unauthorized = (int)HttpStatusCode.Unauthorized;
            context.HttpContext.Response.StatusCode = unauthorized;

            var response = new ApiExceptionDto(unauthorized, errorMessage);
            await context.HttpContext.Response.WriteAsync(response.ToString());
            return;
        }

        await next();
    }
}