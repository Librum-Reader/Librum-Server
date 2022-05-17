using Application.Common.DTOs.Books;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.RequestParameters;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.v1;

public class BookService : IBookService
{
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;

    public BookService(IMapper mapper, IBookRepository bookRepository, IUserRepository userRepository)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }


    public async Task CreateBookAsync(string email, BookInDto bookInDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        if (await _bookRepository.BookAlreadyExists(bookInDto.Title))
        {
            throw new InvalidParameterException("A book with this title already exists");
        }


        await _userRepository.LoadRelationShipsAsync(user);
        var book = _mapper.Map<Book>(bookInDto);
        user.Books.Add(book);

        await _bookRepository.SaveChangesAsync();
    }

    public async Task<IList<BookOutDto>> GetBooksAsync(string email, BookRequestParameter bookRequestParameter)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        var books = _bookRepository.GetBooks(user.Id);
        await _bookRepository.LoadRelationShipsAsync(books);
        
        var bestMatchingBooks = SortByBestMatch(books, bookRequestParameter.SearchString.ToLower());
        var booksFilteredByAuthor = FilterByAuthor(bestMatchingBooks, bookRequestParameter.Author.ToLower());
        var booksFilteredByTimeSinceAdded = FilterByTimeSinceAdded(booksFilteredByAuthor, bookRequestParameter.Added);
        var booksFilteredByFormat = FilterByFormat(booksFilteredByTimeSinceAdded, bookRequestParameter.Format);
        var filteredBooks = FilterByOptions(booksFilteredByFormat, bookRequestParameter);
        var booksFilteredByTag = FilterByTags(filteredBooks, bookRequestParameter.Tag);
        var result = SortByCategories(booksFilteredByTag, bookRequestParameter.SortBy);
        

        return await result.Select(book => _mapper.Map<BookOutDto>(book)).ToListAsync();
    }
    
    private IQueryable<Book> SortByBestMatch(IQueryable<Book> books, string target)
    {
        var sortedBooks =
            from book in books
            let orderController = book.Title.ToLower().StartsWith(target)
                ? 1
                : book.Title.ToLower().Contains(target)
                    ? 2
                    : 3
            orderby orderController, book.Title
            select book;

        return sortedBooks;
    }

    private IQueryable<Book> FilterByAuthor(IQueryable<Book> books, string authorName)
    {
        if (authorName == string.Empty)
        {
            return books;
        }
        
        return books.Where(book => book.Authors
            .Any(author => (author.FirstName.ToLower() + " " + author.LastName.ToLower()).Contains(authorName)));
    }
    
    private IQueryable<Book> FilterByTimeSinceAdded(IQueryable<Book> books, TimeSpan added)
    {
        return books;
    }
    
    private IQueryable<Book> FilterByFormat(IQueryable<Book> books, string format)
    {
        return books;
    }
    
    private IQueryable<Book> FilterByOptions(IQueryable<Book> books, BookRequestParameter bookRequestParameter)
    {
        return books;
    }
    
    private IQueryable<Book> FilterByTags(IQueryable<Book> books, string tag)
    {
        return books;
    }
    
    private IQueryable<Book> SortByCategories(IQueryable<Book> books, BookSortOptions sortOption)
    {
        return sortOption switch
        {
            BookSortOptions.Nothing => books,
            BookSortOptions.RecentlyRead => books.OrderByDescending(book => book.LastOpened),
            BookSortOptions.RecentlyAdded => books.OrderByDescending(book => book.CreationDate),
            BookSortOptions.Percentage => books.OrderByDescending(book => ((double)book.CurrentPage / book.Pages)),
            BookSortOptions.TitleLexicAsc => books.OrderBy(book => book.Title),
            BookSortOptions.TitleLexicDec => books.OrderByDescending(book => book.Title),
            BookSortOptions.AuthorLexicAsc => books
                .OrderBy(book => book.Authors.ElementAtOrDefault(0).FirstName == null)
                .ThenBy(book => book.Authors.ElementAtOrDefault(0).LastName),
            BookSortOptions.AuthorLexicDec => books
                .OrderByDescending(book => book.Authors.ElementAtOrDefault(0).FirstName)
                .ThenByDescending(book => book.Authors.ElementAtOrDefault(0).LastName),
            _ => throw new InvalidParameterException("Selected a not supported 'SortBy' value")
        };
    }
}