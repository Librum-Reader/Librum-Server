using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;


namespace Application.UnitTests.Services;


public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBookBlobStorageManager> _bookBlobStorageManagerMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
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
            _userRepositoryMock.Object, _configurationMock.Object, _bookBlobStorageManagerMock.Object);
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
            PageCount = 1200,
            CurrentPage = 2
        };
    
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });
    
        _bookRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(),
                                                     It.IsAny<Guid>()))
            .ReturnsAsync(false);
        
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
    
    
        // Act
        await _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto);
    
        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ABookService_FailsCreatingABookIfBookAlreadyExists()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            CreationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            Format = "Pdf",
            PageCount = 1200,
            CurrentPage = 2
        };
    
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });
    
        _bookRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(),
                                                     It.IsAny<Guid>()))
            .ReturnsAsync(true);
    
    
        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(() => 
                        _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto));
    }
    
    [Fact]
    public async Task ABookService_FailsCreatingABookIfNoBookStorageAvailable()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            CreationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            Format = "Pdf",
            PageCount = 1200,
            CurrentPage = 2,
            DocumentSize = "10MiB"
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User { Books = new Collection<Book>() });
    
        _bookRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(),
                                                     It.IsAny<Guid>()))
            .ReturnsAsync(false);
        
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _bookRepositoryMock.Setup(x => x.GetUsedBookStorage(It.IsAny<string>()))
            .ReturnsAsync(999999999999); // Library is FULL!
            

            // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _bookService.CreateBookAsync("JohnDoe@gmail.com", bookDto));
    }

    [Fact]
    public async Task ABookService_SucceedsDeletingBooks()
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

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Act
        await _bookService.DeleteBooksAsync("JohnDoe@gmail.com",
                                            bookGuids);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ABookService_SucceedsGettingBooks()
    {
        // Arrange
        var bookGuids = new List<Guid>
        {
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
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
        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(user.Books.AsQueryable());


        // Act
        var result = await _bookService.GetBooksAsync("JohnDoe@gmail.com");

        // Assert
        Assert.Contains(result, book => Guid.Parse(book.Guid) == bookGuids[0]);
        Assert.Contains(result, book => Guid.Parse(book.Guid) == bookGuids[1]);
        Assert.Contains(result, book => Guid.Parse(book.Guid) == bookGuids[2]);
    }
    
    [Fact]
    public async Task ABookService_FailsDeletingBooksIfBooksDontExist()
    {
        // Arrange
        var bookGuids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var user = new User
        {
            Books = new List<Book>
            {
                new Book { BookId = bookGuids[0] },
                new Book { BookId = bookGuids[1] }
            }
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _bookService.DeleteBooksAsync("JohnDoe@gmail.com", bookGuids));
    }

    [Fact]
    public async Task ABookService_SucceedsUpdatingABook()
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
                    Tags = new List<Tag>()
                }
            }
        };

        var bookUpdateDto = new BookForUpdateDto
        {
            Guid = bookGuid,
            Title = "SomeNewTitle"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _bookService.UpdateBookAsync("JohnDoe@gmail.com", bookUpdateDto);

        // Assert
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ABookService_FailsUpdatingABookIfItDoesNotExist()
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
                    Tags = new List<Tag>()
                }
            }
        };

        var bookUpdateDto = new BookForUpdateDto
        {
            Guid = Guid.NewGuid(),
            Title = "SomeNewTitle"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);

        
        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _bookService.UpdateBookAsync("JohnDoe@gmail.com", bookUpdateDto));
    }
}