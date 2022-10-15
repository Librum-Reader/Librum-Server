using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Services.v1;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;


namespace Infrastructure.UnitTests.Services;


public partial class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock = new Mock<IBookRepository>();
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly Mock<ControllerBase> _controllerBaseMock = new Mock<ControllerBase>();
    private readonly IBookService _bookService;


    public BookServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfiles(new List<Profile>
            { new BookAutoMapperProfile(), new AuthorAutoMappingProfile() }));
        var mapper = new Mapper(mapperConfig);

        _bookService = new BookService(mapper, _bookRepositoryMock.Object,
            _userRepositoryMock.Object);
    }


    [Fact]
    public async Task CreateBookAsync_ShouldSaveBook_WhenBookDataValidAndUserExists()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            ReleaseDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            Format = "Pdf",
            Pages = 1200,
            CurrentPage = 2,
            Authors = new Collection<AuthorInDto>
            {
                new AuthorInDto { FirstName = "Someone", LastName = "SomeonesLastName" },
                new AuthorInDto { FirstName = "SomeoneElse", LastName = "Johnson" }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });

        _bookRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto, Guid.NewGuid().ToString());

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        const string firstTagName = "TagOne";
        const string secondTagName = "TagTwo";

        var bookGuid = Guid.NewGuid();
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid, Tags = new List<Tag>() }
            },
            Tags = new List<Tag>
            {
                new Tag { Name = firstTagName },
                new Tag { Name = secondTagName }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", bookGuid.ToString(),
            new List<string> { firstTagName, secondTagName });

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldThrow_WhenTagDoesNotExist()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid, Tags = new List<Tag>() }
            },
            Tags = new List<Tag>()
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", bookGuid.ToString(), new List<string> { "TagOne", "TagTwo" }));
    }
    
    [Fact]
    public async Task AddTagsToBookAsync_ShouldThrow_WhenBookAlreadyHasTag()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        const string tagName = "SomeTag";
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    BookId = bookGuid, 
                    Tags = new List<Tag>
                    {
                        new Tag { Name = tagName }
                    } 
                }
            },
            Tags = new List<Tag>
            {
                new Tag { Name = tagName }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", bookGuid.ToString(), new List<string> { tagName }));
    }

    [Fact]
    public async Task RemoveTagFromBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        const string tagName = "TagOne";
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
                        new Tag { Name = tagName }
                    }
                },
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        await _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com", bookGuid.ToString(), tagName);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBooks_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        var bookGuids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuids[0] },
                new Book { BookId = bookGuids[1] },
                new Book { BookId = bookGuids[2] }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        await _bookService.DeleteBooksAsync("JohnDoe@gmail.com",
            new List<string> { bookGuids[0].ToString(), bookGuids[1].ToString(), bookGuids[2].ToString() });

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBooks_ShouldThrow_WhenABookDoesNotExist()
    {
        // Arrange
        var bookNames = new[] { "FirstBook", "SecondBook", "ThirdBook" };

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = "FirstBook" },
                new Book { Title = "SecondBook" },
                new Book { Title = "ThirdBook" }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _bookService.DeleteBooksAsync("JohnDoe@gmail.com",
            new List<string> { bookNames[0], "ANotExistentBook", bookNames[2] }));
    }

    [Fact]
    public async Task PatchBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid }
            }
        };

        var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
        patchDoc.Operations.Add(new Operation<BookForUpdateDto>("replace", "/Title", "source", "SomeOtherBook"));

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

        _controllerBaseMock.Setup(x => x.TryValidateModel(It.IsAny<ModelStateDictionary>()))
            .Returns(true);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _bookService.PatchBookAsync("JohnDoe@gmail.com", patchDoc, bookGuid.ToString(), _controllerBaseMock.Object);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PatchBookAsync_ShouldThrow_WhenApplyingToBookFails()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid, CurrentPage = -1 }
            }
        };

        var patchDoc = new JsonPatchDocument<BookForUpdateDto>();
        patchDoc.Operations.Add(new Operation<BookForUpdateDto>("replace", "/Title", "source", "SomeOtherBook"));

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

        _controllerBaseMock.Setup(x => x.TryValidateModel(It.IsAny<ModelStateDictionary>()))
            .Returns(false);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _bookService.PatchBookAsync("JohnDoe@gmail.com",
            patchDoc, bookGuid.ToString(), _controllerBaseMock.Object));
    }

    [Fact]
    public async Task AddAuthorToBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        var authorToAdd = new AuthorInDto
        {
            FirstName = "SomeAuthor",
            LastName = "ALastName"
        };

        var bookGuid = Guid.NewGuid();

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuid, Authors = new List<Author>() }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.AddAuthorToBookAsync("JohnDoe@gmail.com", bookGuid.ToString(), authorToAdd);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAuthorFromBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        const string authorFirstName = "AnyFirstName";
        const string authorLastName = "AnyLastname";

        var authorToRemove = new AuthorForRemovalDto
        {
            FirstName = authorFirstName,
            LastName = authorLastName
        };

        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    BookId = bookGuid, 
                    Authors = new List<Author>
                    {
                        new Author { FirstName = authorFirstName, LastName = authorLastName }
                    }
                }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);

        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.RemoveAuthorFromBookAsync("JohnDoe@gmail.com", bookGuid.ToString(), authorToRemove);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}