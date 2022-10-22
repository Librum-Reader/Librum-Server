using System.Net;
using Application.Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Common.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;


    public ExceptionHandlingMiddleware(RequestDelegate next,
                                       IHostEnvironment env,
                                       ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = _env.IsDevelopment()
                ? new ApiExceptionDto(context.Response.StatusCode, ex.Message, ex.StackTrace)
                : new ApiExceptionDto(context.Response.StatusCode, "An error occured");

            await context.Response.WriteAsync(response.ToString());
        }
    }
}