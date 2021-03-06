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

public class ValidateAuthorAttributesTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    
    private readonly Mock<ILogger<ValidateAuthorExistsAttribute>> _authorExistsLoggerMock = new();
    private readonly Mock<ILogger<ValidateAuthorDoesNotExistAttribute>> _authorDoesNotExistLoggerMock = new();

    
    private readonly ValidateAuthorExistsAttribute _authorExistsFilterAttribute;
    private readonly ValidateAuthorDoesNotExistAttribute _authorDoesNotExistFilterAttribute;


    public ValidateAuthorAttributesTests()
    {
        _authorExistsFilterAttribute = new ValidateAuthorExistsAttribute(
            _userRepositoryMock.Object, _bookRepositoryMock.Object, _authorExistsLoggerMock.Object);
        
        _authorDoesNotExistFilterAttribute = new ValidateAuthorDoesNotExistAttribute(
            _userRepositoryMock.Object, _bookRepositoryMock.Object, _authorDoesNotExistLoggerMock.Object);
    }


    [Fact]
    public async Task ValidateAuthorExists_ShouldSucceed_WhenAuthorExists()
    {
        // Arrange
        const string bookTitle = "SomeBook";
        const string authorFirstName = "SomeAuthor";
        const string authorLastName = "SomeLastName";

        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = bookTitle,
                    Authors = new List<Author>
                    {
                        new Author
                        {
                            FirstName = authorFirstName,
                            LastName = authorLastName
                        }
                    }
                }
            }
        };

        var authorInDto = new AuthorForRemovalDto()
        {
            FirstName = authorFirstName,
            LastName = authorLastName
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

        executingContext.ActionArguments.Add("SomeDto", authorInDto);
        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _authorExistsFilterAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateAuthorDoesNotExist_ShouldThrow_WhenAuthorDoesNotExist()
    {
        // Arrange
        const string bookTitle = "SomeBook";

        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = bookTitle,
                    Authors = new List<Author>
                    {
                        new Author
                        {
                            FirstName = "SomeFirstName",
                            LastName = "SomeLastName"
                        }
                    }
                }
            }
        };

        var authorInDto = new AuthorForRemovalDto()
        {
            FirstName = "SomeOtherFirstName",
            LastName = "SomeOtherLastName"
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

        executingContext.ActionArguments.Add("SomeDto", authorInDto);
        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _authorExistsFilterAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateAuthorExists_ShouldThrow_WhenNoDtoWasFound()
    {
        // Arrange
        const string bookTitle = "SomeBook";

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
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => _authorExistsFilterAttribute
            .OnActionExecutionAsync(executingContext, () => Task.FromResult(context)));
    }
    
    
    [Fact]
    public async Task ValidateAuthorExists_ShouldThrow_WhenNoBookTitleWasFound()
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

        executingContext.ActionArguments.Add("SomeDto", new AuthorForRemovalDto());

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => _authorExistsFilterAttribute
            .OnActionExecutionAsync(executingContext, () => Task.FromResult(context)));
    }
    
    
    
    [Fact]
    public async Task ValidateAuthorDoesNotExist_ShouldSucceed_WhenAuthorDoesNotExist()
    {
        // Arrange
        const string bookTitle = "SomeBook";

        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = bookTitle,
                    Authors = new List<Author>
                    {
                        new Author
                        {
                            FirstName = "SomeAuthor",
                            LastName = "SomeLastName"
                        }
                    }
                }
            }
        };

        var authorInDto = new AuthorInDto
        {
            FirstName = "SomeOtherAuthor",
            LastName = "SomeOtherLastName"
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

        executingContext.ActionArguments.Add("SomeDto", authorInDto);
        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _authorDoesNotExistFilterAttribute.OnActionExecutionAsync(executingContext, 
            () => Task.FromResult(context));

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateAuthorDoesNotExist_ShouldThrow_WhenAuthorAlreadyExists()
    {
        // Arrange
        const string bookTitle = "SomeBook";
        const string authorFirstName = "SomeAuthor";
        const string authorLastName = "SomeLastName";

        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = bookTitle,
                    Authors = new List<Author>
                    {
                        new Author
                        {
                            FirstName = authorFirstName,
                            LastName = authorLastName
                        }
                    }
                }
            }
        };

        var authorInDto = new AuthorInDto
        {
            FirstName = authorFirstName,
            LastName = authorLastName
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

        executingContext.ActionArguments.Add("SomeDto", authorInDto);
        executingContext.ActionArguments.Add("bookTitle", bookTitle);

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());
        await _authorDoesNotExistFilterAttribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(context));

        // Assert
        Assert.Equal(400, executingContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task ValidateAuthorDoesNotExist_ShouldThrow_WhenNoDtoWasFound()
    {
        // Arrange
        const string bookTitle = "SomeBook";

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
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => _authorDoesNotExistFilterAttribute
            .OnActionExecutionAsync(executingContext, () => Task.FromResult(context)));
    }
    
    [Fact]
    public async Task ValidateAuthorDoesNotExist_ShouldThrow_WhenNoBookTitleWasFound()
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

        executingContext.ActionArguments.Add("SomeDto", new AuthorInDto());

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());


        // Act
        var context = new ActionExecutedContext(executingContext, new List<IFilterMetadata>(), Mock.Of<Controller>());

        // Assert
        await Assert.ThrowsAsync<InternalServerException>(() => _authorDoesNotExistFilterAttribute
            .OnActionExecutionAsync(executingContext, () => Task.FromResult(context)));
    }
}