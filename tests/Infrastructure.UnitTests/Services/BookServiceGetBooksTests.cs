using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.RequestParameters;
using Domain.Entities;
using Domain.Enums;
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
            SortBy = BookSortOptions.TitleLexicDesc
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
            SortBy = BookSortOptions.AuthorLexicDesc
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

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(data.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));


        // Act
        var result = await _bookService.GetBooksAsync("JohnDoe@gmail.com", requestParameter);

        // Assert
        for (int i = 0; i < data.Count; ++i)
        {
            Assert.Equal(result[i].Title, expectedResult[i].Title);
        }
    }

    [Fact]
    public async Task SortByBestMatch_ShouldSortCorrectly_WhenDataIsValid()
    {
        // Arrange
        const string searchString = "SOME";

        var books = new Collection<Book>
        {
            new Book { Title = "SomeBookTitle" },
            new Book { Title = "AnotherBookTitle" },
            new Book { Title = "MaybeSomeOtherBook" },
            new Book { Title = "ZeLastBook" },
            new Book { Title = "AaTheFirstBook" }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "SomeBookTitle" },
            new Book { Title = "MaybeSomeOtherBook" },
            new Book { Title = "AaTheFirstBook" },
            new Book { Title = "AnotherBookTitle" },
            new Book { Title = "ZeLastBook" }
        };


        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));

        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            SearchString = searchString
        });

        // Assert
        for (int i = 0; i < books.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }
    
    [Fact]
    public async Task FilterByAuthor_ShouldFilterCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book
            {
                Title = "A",
                Authors = new List<Author>
                {
                    new Author { FirstName = "John", LastName = "Doe" }
                }
            },
            new Book { Title = "B", Authors = new List<Author>() },
            new Book
            {
                Title = "C",
                Authors = new List<Author>
                {
                    new Author { FirstName = "Larry", LastName = "Ross" },
                    new Author { FirstName = "Bahmend", LastName = "Ramsi" }
                }
            },
            new Book
            {
                Title = "D",
                Authors = new List<Author>
                {
                    new Author { FirstName = "Boemidi", LastName = "Doe" },
                    new Author { FirstName = "Sam", LastName = "Egals" }
                }
            },
            new Book
            {
                Title = "E",
                Authors = new List<Author>
                {
                    new Author { FirstName = "Relay", LastName = "Doe" },
                    new Author { FirstName = "John", LastName = "Peters" }
                }
            }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "A" },
            new Book { Title = "E" }
        };


        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));

        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            Author = "JOhn"
        });

        // Assert
        for (int i = 0; i < expectedResult.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }

    [Fact]
    public async Task FilterByTimeSinceAdded_ShouldFilterCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book { Title = "A", CreationDate = DateTime.Now.AddMinutes(-40) },
            new Book { Title = "B", CreationDate = DateTime.Now.AddMinutes(-20) },
            new Book { Title = "C", CreationDate = DateTime.Now },
            new Book { Title = "D", CreationDate = DateTime.Now.AddMinutes(-50) },
            new Book { Title = "E", CreationDate = DateTime.Now.AddMinutes(-30) }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "B", CreationDate = DateTime.Now },
            new Book { Title = "C", CreationDate = DateTime.Now.AddMinutes(-20) },
            new Book { Title = "E", CreationDate = DateTime.Now.AddMinutes(-30) },
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        
        
        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            TimePassedAsString = "00:30:00"    // 30 minutes
        });

        // Assert
        for (int i = 0; i < actualResult.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }
    
    [Fact]
    public async Task FilterByFormat_ShouldFilterCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book { Title = "A", Format = BookFormat.Pdf },
            new Book { Title = "B", Format = BookFormat.Mobi },
            new Book { Title = "C", Format = BookFormat.Pdf },
            new Book { Title = "D", Format = BookFormat.Epub },
            new Book { Title = "E", Format = BookFormat.Epub }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "A" },
            new Book { Title = "C" },
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        
        
        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            Format = BookFormat.Pdf
        });

        // Assert
        for (int i = 0; i < actualResult.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }
    
    [Fact]
    public async Task FilterByReadOption_ShouldFilterCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book { Title = "A", Pages = 1200, CurrentPage = 119 },
            new Book { Title = "B", Pages = 491, CurrentPage = 491 },
            new Book { Title = "C", Pages = 332, CurrentPage = 4 },
            new Book { Title = "D", Pages = 921, CurrentPage = 921 },
            new Book { Title = "E", Pages = 267, CurrentPage = 267 }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "B" },
            new Book { Title = "D" },
            new Book { Title = "E" }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        
        
        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            Read = true
        });

        // Assert
        for (int i = 0; i < actualResult.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }
    
    [Fact]
    public async Task FilterByUnReadOption_ShouldFilterCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book { Title = "A", Pages = 1200, CurrentPage = 119 },
            new Book { Title = "B", Pages = 491, CurrentPage = 491 },
            new Book { Title = "C", Pages = 332, CurrentPage = 4 },
            new Book { Title = "D", Pages = 921, CurrentPage = 921 },
            new Book { Title = "E", Pages = 267, CurrentPage = 267 }
        };

        var expectedResult = new Collection<Book>
        {
            new Book { Title = "A" },
            new Book { Title = "C" }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        
        
        // Act
        var actualResult = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            Unread = true
        });

        // Assert
        for (int i = 0; i < actualResult.Count; i++)
        {
            Assert.Equal(expectedResult[i].Title, actualResult[i].Title);
        }
    }
    
    [Fact]
    public async Task PaginateBooks_ShouldPaginateCorrectly_WhenDataIsValid()
    {
        // Arrange
        var books = new Collection<Book>
        {
            new Book { Title = "A", Pages = 1200, CurrentPage = 119 },
            new Book { Title = "B", Pages = 491, CurrentPage = 491 },
            new Book { Title = "C", Pages = 332, CurrentPage = 4 },
            new Book { Title = "D", Pages = 921, CurrentPage = 921 },
            new Book { Title = "E", Pages = 267, CurrentPage = 267 }
        };

        var expectedResultPage1 = new Collection<Book>
        {
            new Book { Title = "A", Pages = 1200, CurrentPage = 119 },
            new Book { Title = "B", Pages = 491, CurrentPage = 491 },
        };
        
        var expectedResultPage2 = new Collection<Book>
        {
            new Book { Title = "C", Pages = 332, CurrentPage = 4 },
            new Book { Title = "D", Pages = 921, CurrentPage = 921 },
        };
        
        var expectedResultPage3 = new Collection<Book>
        {
            new Book { Title = "E", Pages = 267, CurrentPage = 267 }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        _bookRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<string>()))
            .Returns(books.BuildMock());

        _bookRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<Book>()));
        
        
        // Act
        var actualResultPage1 = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            PageNumber = 1,
            PageSize = 2
        });
        
        var actualResultPage2 = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            PageNumber = 2,
            PageSize = 2
        });
        
        var actualResultPage3 = await _bookService.GetBooksAsync(It.IsAny<string>(), new BookRequestParameter
        {
            PageNumber = 3,
            PageSize = 2
        });

        // Assert
        for (int i = 0; i < actualResultPage1.Count; i++)
        {
            Assert.Equal(expectedResultPage1[i].Title, actualResultPage1[i].Title);
        }
        
        for (int i = 0; i < actualResultPage2.Count; i++)
        {
            Assert.Equal(expectedResultPage2[i].Title, actualResultPage2[i].Title);
        }
        
        for (int i = 0; i < actualResultPage3.Count; i++)
        {
            Assert.Equal(expectedResultPage3[i].Title, actualResultPage3[i].Title);
        }
    }
}