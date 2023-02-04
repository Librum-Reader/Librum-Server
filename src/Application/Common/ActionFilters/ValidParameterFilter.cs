using System.Net;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidParameterFilter : IAsyncActionFilter
{
    private readonly ILogger<ValidParameterFilter> _logger;


    public ValidParameterFilter(ILogger<ValidParameterFilter> logger)
    {
        _logger = logger;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next)
    {
        if (context.ActionArguments.Any(arg => arg.Value == null ||
                                               IsInvalidString(arg.Value)))
        {
            var errorMessage = "The given string parameter was invalid";
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

    private bool IsInvalidString<T>(T value)
    {
        if (value is not string str)
        {
            return false;
        }

        return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }
}