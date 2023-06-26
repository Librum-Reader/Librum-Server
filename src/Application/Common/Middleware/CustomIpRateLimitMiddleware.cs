using Application.Common.DTOs;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Application.Common.Middleware;

public class CustomIpRateLimitMiddleware: IpRateLimitMiddleware
{
    public CustomIpRateLimitMiddleware(RequestDelegate next, 
                                       IProcessingStrategy strategy,
                                       IOptions<IpRateLimitOptions> options,
                                       IIpPolicyStore policyStore,
                                       IRateLimitConfiguration config,
                                       ILogger<IpRateLimitMiddleware> logger) 
        : base(next, strategy, options, policyStore, config, logger)
    {
    }

    public override Task ReturnQuotaExceededResponse(HttpContext httpContext, 
                                                     RateLimitRule rule, 
                                                     string retryAfter)
    {
        var message = $"Too many requests. Limit is: {rule.Limit} per {rule.Period}. " +
                      $"Retry after {retryAfter}s";
        var response = new CommonErrorDto(429, message, 19);

        httpContext.Response.Headers["Retry-After"] = retryAfter;
        httpContext.Response.StatusCode = 429;
        httpContext.Response.ContentType = "application/json";

        return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}