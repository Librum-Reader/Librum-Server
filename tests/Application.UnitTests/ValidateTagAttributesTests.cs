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

namespace Application.UnitTests;

public class ValidateTagAttributesTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<ValidateBookExistsAttribute>> _bookExistsLoggerMock = new();

    private readonly ValidateBookHasTagAttribute _filterAttribute;


    public ValidateTagAttributesTests()
    {
        // _filterAttribute = new ValidateBookHasTagAttribute(_userRepositoryMock.Object, 
        //     _bookExistsLoggerMock.Object);
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
        // await _bookExistsFilterAttribute.OnActionExecutionAsync(executingContext, async () => context);

        // Assert
        Assert.Equal(200, executingContext.HttpContext.Response.StatusCode);
    }
    
}