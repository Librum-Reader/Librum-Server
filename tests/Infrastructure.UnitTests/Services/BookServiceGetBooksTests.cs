using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.RequestParameters;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class SortDataProvider : TheoryData<Collection<Book>, Collection<Book>, BookRequestParameter>
{
    public SortDataProvider()
    {
        // Not sorted
        var notSortedResult = new Collection<Book>
        {
            new Book { Title = "B" },
            new Book { Title = "A" },
            new Book { Title = "C" }
        };
        var notSortedData = new Collection<Book>
        {
            new Book { Title = "B" },
            new Book { Title = "A" },
            new Book { Title = "C" }
        };
        
        var notSortedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.Nothing
        };
        
        Add(notSortedResult, notSortedData, notSortedRequest);
        
        // Ascendantly by title
        var ascendantlyByTitleSortedResult = new Collection<Book>
        {
            new Book { Title = "ATitle" },
            new Book { Title = "BTitle" },
            new Book { Title = "CTitle" },
            new Book { Title = "DTitle" }
        };
        var ascendantlyByTitleSortedData = new Collection<Book>
        {
            new Book { Title = "DTitle" },
            new Book { Title = "ATitle" },
            new Book { Title = "CTitle" },
            new Book { Title = "BTitle" }
        };
        
        var ascendantlyByTitleSortedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.TitleLexicAsc
        };
        
        Add(ascendantlyByTitleSortedResult, ascendantlyByTitleSortedData, ascendantlyByTitleSortedRequest);
        
        
        
        // Descendantly by title
        var descendantlyByTitleSortedResult = new Collection<Book>
        {
            new Book { Title = "DTitle" },
            new Book { Title = "CTitle" },
            new Book { Title = "BTitle" },
            new Book { Title = "ATitle" }
        };
        var descendantlyByTitleSortedData = new Collection<Book>
        {
            new Book { Title = "DTitle" },
            new Book { Title = "ATitle" },
            new Book { Title = "CTitle" },
            new Book { Title = "BTitle" }
        };
        
        var descendantlyByTitleSortedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.TitleLexicDec
        };
        
        Add(descendantlyByTitleSortedResult, descendantlyByTitleSortedData, descendantlyByTitleSortedRequest);
        
        
        
        // Ascendantly by author
        var ascendantlyByAuthorSortedResult = new Collection<Book>
        {
            new Book { Title = "BookOne", },
            new Book { Title = "BookTwo", },
            new Book { Title = "BookThree", },
            new Book { Title = "BookFour", },
            new Book { Title = "BookFive" }
        };
        var ascendantlyByAuthorSortedData = new Collection<Book>
        {
            new Book
            {
                Title = "BookThree",
                Authors = new List<Author> { new Author { FirstName = "BnAuthor", LastName = "Xxx" } }
            },
            new Book
            {
                Title = "BookFive",
                Authors = new List<Author> { new Author { FirstName = "SnAuthor", LastName = "Xxx" } }
            },
            new Book
            {
                Title = "BookOne",
                Authors = new List<Author> { new Author { FirstName = "AnAuthor", LastName = "Aaa" } }
            },
            new Book
            {
                Title = "BookFour",
                Authors = new List<Author> { new Author { FirstName = "BnAuthor", LastName = "Zzz" } }
            },
            new Book
            {
                Title = "BookTwo",
                Authors = new List<Author> { new Author { FirstName = "AnAuthor", LastName = "Bbb" } }
            }
        };
        
        var ascendantlyByAuthorSortedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.AuthorLexicAsc
        };
        
        Add(ascendantlyByAuthorSortedResult, ascendantlyByAuthorSortedData, ascendantlyByAuthorSortedRequest);
        
        
        
        // Descendantly by author
        var descendantlyByAuthorSortedResult = new Collection<Book>
        {
            new Book { Title = "BookFive" },
            new Book { Title = "BookFour", },
            new Book { Title = "BookThree", },
            new Book { Title = "BookTwo", },
            new Book { Title = "BookOne", }
        };
        var descendantlyByAuthorSortedData = new Collection<Book>
        {
            new Book
            {
                Title = "BookThree",
                Authors = new List<Author> { new Author { FirstName = "BnAuthor", LastName = "Xxx" } }
            },
            new Book
            {
                Title = "BookFive",
                Authors = new List<Author> { new Author { FirstName = "SnAuthor", LastName = "Xxx" } }
            },
            new Book
            {
                Title = "BookOne",
                Authors = new List<Author> { new Author { FirstName = "AnAuthor", LastName = "Aaa" } }
            },
            new Book
            {
                Title = "BookFour",
                Authors = new List<Author> { new Author { FirstName = "BnAuthor", LastName = "Zzz" } }
            },
            new Book
            {
                Title = "BookTwo",
                Authors = new List<Author> { new Author { FirstName = "AnAuthor", LastName = "Bbb" } }
            }
        };
        
        var descendantlyByAuthorSortedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.AuthorLexicDec
        };
        
        Add(descendantlyByAuthorSortedResult, descendantlyByAuthorSortedData, descendantlyByAuthorSortedRequest);
        
        
        // Recently read
        var recentlyReadResult = new Collection<Book>
        {
            new Book { Title = "A", LastOpened = DateTime.Now },
            new Book { Title = "B", LastOpened = DateTime.Now.AddMinutes(-10) },
            new Book { Title = "C", LastOpened = DateTime.Now.AddMinutes(-50) },
            new Book { Title = "D", LastOpened = DateTime.Now.AddMinutes(-51) }
        };
        var recentlyReadData = new Collection<Book>
        {
            new Book { Title = "B", LastOpened = DateTime.Now.AddMinutes(-10) },
            new Book { Title = "C", LastOpened = DateTime.Now.AddMinutes(-50) },
            new Book { Title = "A", LastOpened = DateTime.Now },
            new Book { Title = "D", LastOpened = DateTime.Now.AddMinutes(-51) }
        };
        
        var recentlyReadRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.RecentlyRead
        };
        
        Add(recentlyReadResult, recentlyReadData, recentlyReadRequest);
        
        
        
        // Recently added
        var recentlyAddedResult = new Collection<Book>
        {
            new Book { Title = "A", CreationDate = DateTime.Now },
            new Book { Title = "B", CreationDate = DateTime.Now.AddMinutes(-11) },
            new Book { Title = "C", CreationDate = DateTime.Now.AddMinutes(-24) },
            new Book { Title = "D", CreationDate = DateTime.Now.AddMinutes(-26) }
        };
        var recentlyAddedData = new Collection<Book>
        {
            new Book { Title = "B", CreationDate = DateTime.Now.AddMinutes(-11) },
            new Book { Title = "C", CreationDate = DateTime.Now.AddMinutes(-24) },
            new Book { Title = "A", CreationDate = DateTime.Now },
            new Book { Title = "D", CreationDate = DateTime.Now.AddMinutes(-26) }
        };
        
        var recentlyAddedRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.RecentlyAdded
        };
        
        Add(recentlyAddedResult, recentlyAddedData, recentlyAddedRequest);
        
        
        
        // Percentage read
        var percentageReadResult = new Collection<Book>
        {
            new Book { Title = "A", Pages = 241, CurrentPage = 241 },
            new Book { Title = "B", Pages = 1649, CurrentPage = 241 },
            new Book { Title = "C", Pages = 241, CurrentPage = 3 },
            new Book { Title = "D", Pages = 612, CurrentPage = 4 }
        };
        var percentageReadData = new Collection<Book>
        {
            new Book { Title = "C", Pages = 241, CurrentPage = 3 },
            new Book { Title = "A", Pages = 241, CurrentPage = 241 },
            new Book { Title = "B", Pages = 1649, CurrentPage = 241 },
            new Book { Title = "D", Pages = 612, CurrentPage = 4 }
        };
        
        var percentageReadRequest = new BookRequestParameter()
        {
            SortBy = BookSortOptions.Percentage
        };
        
        Add(percentageReadResult, percentageReadData, percentageReadRequest);
    }
}

public partial class BookServiceTests
{
    [Theory]
    [ClassData(typeof(SortDataProvider))]
    public async Task GetBooksAsync_ShouldGetCorrectResults_WhenDataIsValid(Collection<Book> expectedResult,
        Collection<Book> data, BookRequestParameter requestParameter)
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetBooks(It.IsAny<string>()))
            .Returns(data.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        

        // Act
        var result = await _bookService.GetBooksAsync("JohnDoe@gmail.com", requestParameter);
        
        // Assert
        for(int i = 0; i < data.Count; ++i)
        {
            Assert.Equal(result[i].Title, expectedResult[i].Title);
        }
    }
}