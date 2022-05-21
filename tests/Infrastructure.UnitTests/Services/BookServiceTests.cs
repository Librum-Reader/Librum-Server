using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services.v1;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public partial class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock = new Mock<IBookRepository>();
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly Mock<ITagRepository> _tagRepositoryMock = new Mock<ITagRepository>();
    private readonly IMapper _mapper;
    private readonly IBookService _bookService;


    public BookServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfiles(new List<Profile>
            { new BookAutoMapperProfile(), new AuthorAutoMappingProfile() }));
        _mapper = new Mapper(mapperConfig);

        _bookService = new BookService(_mapper, _bookRepositoryMock.Object,
            _userRepositoryMock.Object, _tagRepositoryMock.Object);
    }


    [Fact]
    public async Task CreateBookAsync_ShouldSaveBook_WhenBookDataValidAndUserExists()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            ReleaseDate = DateTime.Now,
            Format = BookFormats.Pdf,
            Pages = 1200,
            CurrentPage = 2,
            Authors = new Collection<AuthorInDto>()
        };

        for (int i = 0; i < 14; ++i)
        {
            bookDto.Authors.Add(new AuthorInDto { FirstName = "Someone", LastName = "SomeonesLastName" });
        }

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });
        
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);


        // Act
        await _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.CreateBookAsync("JohnDoe@gmail.com", new BookInDto()));
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrow_WhenABookWithTheTitleAlreadyExists()
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


        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.CreateBookAsync("JohnDoe@gmail.com", new BookInDto { Title = bookTitle }));
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        const string firstTagName = "TagOne";
        const string secondTagName = "TagTwo";

        const string bookName = "SomeBook";
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = bookName, Tags = new List<Tag>() }
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
        await _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", bookName,
            new List<string> { firstTagName, secondTagName });

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", "SomeBook", new List<string>()));
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldThrow_WhenBookDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new List<Book>() });
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", "SomeBook", new List<string> { "MyTag" }));
    }

    [Fact]
    public async Task AddTagsToBookAsync_ShouldThrow_WhenTagDoesNotExist()
    {
        // Arrange
        const string bookName = "SomeBook";
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { Title = "SomeBook", Tags = new List<Tag>() }
            },
            Tags = new List<Tag>()
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.AddTagsToBookAsync("JohnDoe@gmail.com", bookName, new List<string> { "TagOne", "TagTwo" }));
    }

    [Fact]
    public async Task RemoveTagFromBookAsync_ShouldCallSaveChangesAsync_WhenDataIsValid()
    {
        // Arrange
        const string tagName = "TagOne";
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = "SomeBook",
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
        await _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com", "SomeBook", tagName);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveTagFromBookAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com", "SomeBook", "TagOne"));
    }

    [Fact]
    public async Task RemoveTagFromBookAsync_ShouldThrow_WhenBookDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new List<Book>() });
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com", "SomeBook", "TagOne"));
    }

    [Fact]
    public async Task RemoveTagFromBookAsync_ShouldThrow_WhenTagDoesNotExist()
    {
        // Arrange
        var user = new User
        {
            Books = new List<Book>
            {
                new Book
                {
                    Title = "SomeBook",
                    Tags = new List<Tag>()
                },
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() =>
            _bookService.RemoveTagFromBookAsync("JohnDoe@gmail.com", "SomeBook", "TagOne"));
    }
}