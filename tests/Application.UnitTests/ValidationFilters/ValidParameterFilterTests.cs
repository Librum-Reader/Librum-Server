using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.ValidationFilters;

public class ValidParameterFilterTests
{
    private readonly Mock<ILogger<ValidParameterFilter>> _loggerMock = new();
    private readonly ValidParameterFilter _filterFilter;
    

    public ValidParameterFilterTests()
    {
        _filterFilter = new ValidParameterFilter(_loggerMock.Object);
    }


    [Fact]
    public async Task AValidParameterAttribute_Succeeds()
    {
        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("SomeValidString",
                                             "this is an valid string");
        executingContext.ActionArguments.Add("AnotherValidString",
                                             "SomePassword123");

        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _filterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task AValidParameterAttribute_FailsIfNoParameterExists()
    {
        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("SomeValidString", new User());
        executingContext.ActionArguments.Add("AnotherValidString", 32);

        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _filterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task AValidParameterAttribute_FailsIfParameterIsNull()
    {
        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("SomeValidString", null);

        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _filterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task AValidParameterAttribute_FailsIfParameterIsOnlyWhitespaces()
    {
        var modelState = new ModelStateDictionary();
        var httpContextMock = new DefaultHttpContext();

        var actionContext = new ActionContext(
            httpContextMock,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            modelState
        );

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            modelState
        );

        executingContext.ActionArguments.Add("SomeValidString", "   ");

        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _filterFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
}