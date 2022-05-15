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
using Infrastructure.Services.v1;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock = new Mock<IBookRepository>();
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly IMapper _mapper;
    private readonly IBookService _bookService;
    
    
    public BookServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfiles(new List<Profile> 
            { new BookAutoMapperProfile(), new AuthorAutoMappingProfile() }));
        _mapper = new Mapper(mapperConfig);

        _bookService = new BookService(_mapper, _bookRepositoryMock.Object, _userRepositoryMock.Object);
    }


    [Fact]
    public async Task CreateBookAsync_ShouldSaveBook_WhenBookDataValidAndUserExists()
    {
        // Arrange
        var bookDto = new BookInDto
        {
            Title = "Some book",
            ReleaseDate = DateTime.Now,
            Format = "PDF",
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

        _userRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<User>()));
        
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
        await Assert.ThrowsAsync<InvalidParameterException>(() => _bookService.CreateBookAsync("JohnDoe@gmail.com", new BookInDto()));
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrow_WhenABookWithTheTitleAlreadyExists()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());
        
        _bookRepositoryMock.Setup(x => x.BookAlreadyExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _bookService.CreateBookAsync("JohnDoe@gmail.com", new BookInDto()));
    }
}