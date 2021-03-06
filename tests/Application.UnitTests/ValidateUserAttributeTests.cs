using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
using Application.Interfaces.Repositories;
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

namespace Application.UnitTests;

public class ValidateUserAttributeTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<ValidateUserExistsAttribute>> _loggerMock = new();

    private readonly ValidateUserExistsAttribute _filterAttribute;


    public ValidateUserAttributeTests()
    {
        _filterAttribute = new ValidateUserExistsAttribute(_userRepositoryMock.Object, _loggerMock.Object);
    }
    
    
    [Fact]
    public async Task ValidateUserExists_ShouldSucceed_WhenUserExists()
    {
        // Arrange
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
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _filterAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateUserExists_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
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
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _filterAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(401, executingContext.HttpContext.Response.StatusCode);
    }
}