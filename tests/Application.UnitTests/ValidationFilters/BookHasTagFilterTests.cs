using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.ActionFilters;
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

namespace Application.UnitTests.ValidationFilters;

public class BookHasTagFilterTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<ILogger<BookHasTagFilter>> _loggerMock = new();

    private readonly BookHasTagFilter _bookHasTagFilter;

    public BookHasTagFilterTests()
    {
        _bookHasTagFilter = new BookHasTagFilter(_userRepositoryMock.Object,
                                                       _bookRepositoryMock.Object,
                                                       _loggerMock.Object);
    }
    
    
    [Fact]
    public async Task ABookHasTagAttribute_Succeeds()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        var tagGuid = Guid.NewGuid();
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    BookId = bookGuid,
                    Tags = new List<Tag>
                    {
                        new Tag
                        {
                            TagId = tagGuid,
                            Name = "SomeTag"
                        }
                    }
                }
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

        executingContext.ActionArguments.Add("tagGuid", tagGuid.ToString());
        executingContext.ActionArguments.Add("bookGuid", bookGuid.ToString());
        

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _bookHasTagFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ABookHasTagAttribute_FailsIfBookDoesNotHaveTag()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    BookId = bookGuid,
                    Tags = new List<Tag>
                    {
                        new Tag
                        {
                            TagId = Guid.NewGuid(),
                            Name = "SomeTag"
                        }
                    }
                }
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

        executingContext.ActionArguments.Add("tagGuid", Guid.NewGuid().ToString());
        executingContext.ActionArguments.Add("bookGuid", bookGuid.ToString());
        

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await _bookHasTagFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ABookHasTagAttribute_FailsIfNoBookGuidParameterExists()
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

        executingContext.ActionArguments.Add("tagGuid", Guid.Empty.ToString());


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _bookHasTagFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context)));
        
    }
    
    [Fact]
    public async Task ABookHasTagAttribute_FailsIfNoTagGuidParameterExists()
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
        
        executingContext.ActionArguments.Add("bookGuid", Guid.Empty.ToString());


        // Act
        var context = new ActionExecutedContext(executingContext,
                                                new List<IFilterMetadata>(),
                                                Mock.Of<Controller>());
        await Assert.ThrowsAsync<InternalServerException>(() => 
            _bookHasTagFilter.OnActionExecutionAsync(executingContext,
                    () => Task.FromResult(context)));
        
    }
}