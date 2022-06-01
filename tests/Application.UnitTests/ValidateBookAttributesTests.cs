using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
using Application.Common.DTOs.Authors;
using Application.Common.Exceptions;
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

public class ValidateBookAttributesTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<ValidateBookExistsAttribute>> _bookExistsLoggerMock = new();
    private readonly Mock<ILogger<ValidateBookDoesNotExistAttribute>> _bookDoesNotExistLoggerMock = new();

    private readonly ValidateBookExistsAttribute _bookExistsFilterAttribute;
    private readonly ValidateBookDoesNotExistAttribute _bookDoesNotExistFilterAttribute;


    public ValidateBookAttributesTests()
    {
        _bookExistsFilterAttribute = new ValidateBookExistsAttribute(_userRepositoryMock.Object, 
            _bookExistsLoggerMock.Object);

        _bookDoesNotExistFilterAttribute = new ValidateBookDoesNotExistAttribute(_userRepositoryMock.Object,
            _bookDoesNotExistLoggerMock.Object);
    }


    [Fact]
    public async Task ValidateBookExists_ShouldSucceed_WhenBookExists()
    {
        // Arrange
        const string bookTitle = "SomeBook";
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = bookTitle }
            }
        };

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

        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookExistsFilterAttribute.OnActionExecutionAsync(executingContext, async () => context);

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookExists_ShouldFail_WhenBookDoesNotExist()
    {
        // Arrange
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = "SomeBookTitle" }
            }
        };

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

        executingContext.ActionArguments.Add("bookTitle", "AnotherBookTitle");

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookExistsFilterAttribute.OnActionExecutionAsync(executingContext, async () => context);

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookExists_ShouldThrow_WhenNoBookTitleFound()
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


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _bookExistsFilterAttribute.OnActionExecutionAsync(executingContext, async () => context));
    }
    
    
    
    
    [Fact]
    public async Task ValidateBookDoesNotExist_ShouldSucceed_WhenBookDoesNotExists()
    {
        // Arrange
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = "SomeBook" }
            }
        };

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

        executingContext.ActionArguments.Add("bookTitle", "SomeOtherBook");

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookDoesNotExistFilterAttribute.OnActionExecutionAsync(executingContext, async () => context);

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookDoesNotExist_ShouldFail_WhenBookExists()
    {
        // Arrange
        const string bookTitle = "SomeBook";
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = bookTitle }
            }
        };

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

        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _bookDoesNotExistFilterAttribute.OnActionExecutionAsync(executingContext, async () => context);

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateBookDoesNotExist_ShouldThrow_WhenNoBookTitleFound()
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
        
        
        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _bookDoesNotExistFilterAttribute.OnActionExecutionAsync(executingContext, async () => context));
    }
}