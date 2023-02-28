using System;
using System.Collections.Generic;
using System.Linq;
using Application.Common.DTOs.Books;
using Application.Common.DTOs.Tags;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Xunit;

namespace Application.UnitTests.AutoMappings;

public class BookAutoMapperTests
{
    private IMapper _bookAutoMapper;
    
    public BookAutoMapperTests()
    {
        var mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfiles(new List<Profile>
            {
                new BookAutoMapperProfile(),
                new TagAutoMapperProfile()
            }));
        _bookAutoMapper = new Mapper(mapperConfig);
    }


    [Fact]
    public void ABookAutoMapper_SucceedsMappingABookInDtoToABook()
    {
        // Arrange
        string dateTimeFormat = "hh:mm:ss - dd.MM.yyyy";
        var bookInDto = new BookInDto
        {
            Title = "SomeBook",
            Guid = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow.ToString(dateTimeFormat),
            CurrentPage = 2,
            Tags = new List<TagInDto>()
            {
                new TagInDto { Guid = Guid.NewGuid(), Name = "SomeTag" }
            },
            PageCount = 200,
            Format = "Pdf",
            Authors = "Bob martin",
            // Cover = null,
            Creator = "SomeCreator",
            Data = null,
            Language = "German",
            DocumentSize = "5MiB",
            LastModified = DateTime.UtcNow.AddDays(-1).ToString(dateTimeFormat),
            LastOpened = "",
            PagesSize = "SomeSize",
            AddedToLibrary = DateTime.UtcNow.AddDays(-1).ToString(dateTimeFormat)
        };

        // Act
        var result = _bookAutoMapper.Map<Book>(bookInDto);

        // Assert
        Assert.Equal(bookInDto.Title, result.Title);
        Assert.Equal(bookInDto.CreationDate, result.CreationDate);
        Assert.Equal(bookInDto.CurrentPage, result.CurrentPage);
        Assert.Equal(bookInDto.PageCount, result.PageCount);
        Assert.Equal(bookInDto.Format, result.Format);
        Assert.Equal(bookInDto.Authors, result.Authors);
        // Assert.Equal("none", result.CoverLink);
        Assert.Equal(bookInDto.Creator, result.Creator);
        Assert.Equal(bookInDto.DocumentSize, result.DocumentSize);
        Assert.Equal(bookInDto.LastModified, result.LastModified);
        Assert.Equal(bookInDto.LastOpened, result.LastOpened);
        Assert.Equal(bookInDto.PagesSize, result.PagesSize);
        Assert.Equal(bookInDto.AddedToLibrary, result.AddedToLibrary);
    }
    
    [Fact]
    public void ABookAutoMapper_SucceedsMappingBookToABookOutDto()
    {
        // Arrange
        string dateTimeFormat = "hh:mm:ss - dd.MM.yyyy";
        var book = new Book
        {
            BookId = Guid.NewGuid(),
            Title = "SomeBook",
            CreationDate = DateTime.UtcNow.ToString(dateTimeFormat),
            CurrentPage = 2,
            Tags = new List<Tag>()
            {
                new Tag { TagId = Guid.NewGuid(), Name = "SomeTag" },
                new Tag { TagId = Guid.NewGuid(), Name = "AnotherTag" }
            },
            PageCount = 200,
            Format = "Pdf",
            Authors = "Bob martin",
            CoverLink = "none",
            Creator = "SomeCreator",
            DataLink = "none",
            Language = "German",
            DocumentSize = "5MiB",
            LastModified = DateTime.UtcNow.AddDays(-1).ToString(dateTimeFormat),
            LastOpened = "",
            PagesSize = "SomeSize",
            AddedToLibrary = DateTime.UtcNow.AddDays(-1).ToString(dateTimeFormat)
        };

        // Act
        var result = _bookAutoMapper.Map<BookOutDto>(book);

        // Assert
        Assert.Equal(book.BookId.ToString(), result.Guid);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.CreationDate, result.CreationDate);
        Assert.Equal(book.CurrentPage, result.CurrentPage);
        Assert.Equal(book.PageCount, result.PageCount);
        Assert.Equal(book.Format, result.Format);
        Assert.Equal(book.Authors, result.Authors);
        Assert.Equal(book.Creator, result.Creator);
        Assert.Equal(book.DocumentSize, result.DocumentSize);
        Assert.Equal(book.LastModified, result.LastModified);
        Assert.Equal(book.LastOpened, result.LastOpened);
        Assert.Equal(book.PagesSize, result.PagesSize);
        Assert.Equal(book.AddedToLibrary, result.AddedToLibrary);

        Assert.All(book.Tags, tag => 
                       result.Tags.Any(resultTag => 
                                           resultTag.Guid == tag.TagId.ToString() && 
                                           resultTag.Name == tag.Name));
    }
}