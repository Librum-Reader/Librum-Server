using System.Net;
using Application.Common.DTOs;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Application.Common.ActionFilters;

public class ValidateParameterIsValidAttribute : IAsyncActionFilter
{
    private readonly ILogger<ValidateParameterIsValidAttribute> _logger;


    public ValidateParameterIsValidAttribute(ILogger<ValidateParameterIsValidAttribute> logger)
    {
        _logger = logger;
    }
    

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments)
        {
            if(arg.Value == null || (arg.Value is string value && string.IsNullOrWhiteSpace(value)))
            {
                _logger.LogWarning("A book with this title already exists");
            
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            
                var response = new ApiExceptionDto(context.HttpContext.Response.StatusCode, 
                    "A book with this title already exists");

                await context.HttpContext.Response.WriteAsync(response.ToString());
                return;
            }
        }
            
        await next();
    }
}