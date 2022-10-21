using System.Net;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidParameterAttribute : IAsyncActionFilter
{
    private readonly ILogger<ValidParameterAttribute> _logger;


    public ValidParameterAttribute(ILogger<ValidParameterAttribute> logger)
    {
        _logger = logger;
    }
    
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments)
        {
            if (arg.Value == null || IsInvalidString(arg.Value))
            {
                _logger.LogWarning("The given string parameter was invalid");
            
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
                var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                    "The given string parameter was invalid");

                await context.HttpContext.Response.WriteAsync(response.ToString());
                return;
            }
        }
        
        await next();
    }

    private bool IsInvalidString<T>(T value)
    {
        if (value is not string str)
        {
            return false;
        }

        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            return true;
        }

        return false;
    }
}