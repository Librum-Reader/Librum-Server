using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services.v1;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;


namespace Application.UnitTests.Services;


public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ControllerBase> _controllerBaseMock = new();
    private readonly IBookService _bookService;


    public BookServiceTests()
    {
        var mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfiles(new List<Profile>
            {
                new BookAutoMapperProfile()
            }));
        var mapper = new Mapper(mapperConfig);

        _bookService = new BookService(mapper, _bookRepositoryMock.Object,
            _userRepositoryMock.Object);
    }


    [Fact]
    public async Task ABookService_SucceedsCreatingABook()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            CreationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            Format = "Pdf",
            Pages = 1200,
            CurrentPage = 2
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });

        _bookRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(),
                                                     It.IsAny<string>()))
            .ReturnsAsync(false);
        
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto,
                                           Guid.NewGuid().ToString());

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ABookService_SucceedsAddingTagsToBook()
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
        
        var tagList = new List<string>
        {
            firstTagName, 
            secondTagName
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);

        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.AddTagsToBookAsync("JohnDoe@gmail.com",
                                              bookGuid.ToString(),
                                              tagList);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ABookService_FailsAddingTagsToBookIfTagDoesNotExist()
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

        var tagList = new List<string> { "TagOne", "TagTwo" };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com",
                                            bookGuid.ToString(),
                                            tagList));
    }
    
    [Fact]
    public async Task ABookService_FailsAddingTagsToBookIfTagsAlreadyExist()
    {
        // Arrange
        var bookGuid = Guid.NewGuid();
        var tagNames = new List<string> { "SomeTag" };
        
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    BookId = bookGuid, 
                    Tags = new List<Tag>
                    {
                        new Tag { Name = tagNames[0] }
                    } 
                }
            },
            Tags = new List<Tag>
            {
                new Tag { Name = tagNames[0] }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com",
                                            bookGuid.ToString(),
                                            tagNames));
    }

    [Fact]
    public async Task ABookService_SucceedsRemovingTagFromBook()
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

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        await _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com",
                                                  bookGuid.ToString(),
                                                  tagName);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ABookService_SucceedsDeletingBooks()
    {
        // Arrange
        var bookGuids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var bookGuidsAsString = new List<string>
        {
            bookGuids[0].ToString(),
            bookGuids[1].ToString(),
            bookGuids[2].ToString()
        };

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuids[0] },
                new Book { BookId = bookGuids[1] },
                new Book { BookId = bookGuids[2] }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        await _bookService.DeleteBooksAsync("JohnDoe@gmail.com",
                                            bookGuidsAsString);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ABookService_FailsDeletingBooksIfBooksDontExist()
    {
        // Arrange
        var bookNames = new[] { "FirstBook", "SecondBook", "ANonExistentBook" };

        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = bookNames[0] },
                new Book { Title = bookNames[1] }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _bookService.DeleteBooksAsync("JohnDoe@gmail.com",
                                                bookNames));
    }

    [Fact]
    public async Task ABookService_SucceedsPatchingABook()
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

        var bookUpdateDto = new BookForUpdateDto
        {
            Title = "SomeNewTitle"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _bookService.PatchBookAsync("JohnDoe@gmail.com", bookUpdateDto,
                                          bookGuid.ToString());

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}