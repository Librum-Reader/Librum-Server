using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Extensions;
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

    public BookService(IMapper mapper, IBookRepository bookRepository, 
        IUserRepository userRepository)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }


    public async Task CreateBookAsync(string email, BookInDto bookInDto)
    {
        var user = await CheckIfUserExistsAsync(email, trackChanges: true);

        if (BookExists(user, bookInDto.Title))
        {
            throw new InvalidParameterException("A book with this title already exists");
        }


        var book = _mapper.Map<Book>(bookInDto);
        user.Books.Add(book);

        await _bookRepository.SaveChangesAsync();
    }
    
    private bool BookExists(User user, string bookName) => user.Books.Any(book => book.Title == bookName);
    
    public async Task<IList<BookOutDto>> GetBooksAsync(string email, BookRequestParameter bookRequestParameter)
    {
        var user = await CheckIfUserExistsAsync(email, trackChanges: false);

        var books = _bookRepository.GetBooks(user.Id);
        await _bookRepository.LoadRelationShipsAsync(books);
        

        var processedBooks = books
            .FilterByTags(bookRequestParameter.Tag?.Name)
            .FilterByAuthor(bookRequestParameter.Author.ToLower())
            .FilterByTimeSinceAdded(bookRequestParameter.TimePassed)
            .FilterByFormat(bookRequestParameter.Format)
            .FilterByOptions(bookRequestParameter)
            .SortByBestMatch(bookRequestParameter.SearchString.ToLower())
            .SortByCategories(bookRequestParameter.SortBy, bookRequestParameter.SearchString)
            .PaginateBooks(bookRequestParameter.PageNumber, bookRequestParameter.PageSize);

        // var a = books.SelectMany(book => book.Tags).OrderBy(x => x.Name);
        
        return await processedBooks.Select(book => _mapper.Map<BookOutDto>(book)).ToListAsync();
    }

    public async Task AddTagsToBookAsync(string email, string bookTitle, IEnumerable<string> tagNames)
    {
        var user = await CheckIfUserExistsAsync(email, trackChanges: true);
        
        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        if (book == null)
        {
            throw new InvalidParameterException("No book with this name exists");
        }
        await _bookRepository.LoadRelationShipsAsync(book);
        
        foreach (var tagName in tagNames)
        {
            var tag = GetTagIfDoesNotExist(user, tagName);
            book.Tags.Add(tag);
        }

        await _bookRepository.SaveChangesAsync();
    }

    private Tag GetTagIfDoesNotExist(User user, string tagName)
    {
        var tag = user.Tags.SingleOrDefault(tag => tag.Name == tagName);
        if (tag == null)
        {
            throw new InvalidParameterException("No tag called " + tagName + " exists");
        }

        return tag;
    }

    public async Task RemoveTagFromBookAsync(string email, string bookTitle, string tagName)
    {
        var user = await CheckIfUserExistsAsync(email, trackChanges: true);

        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        if (book == null)
        {
            throw new InvalidParameterException("No book with this title exists");
        }

        await _bookRepository.LoadRelationShipsAsync(book);

        var tag = book.Tags.SingleOrDefault(tag => tag.Name == tagName);
        if (tag == null)
        {
            throw new InvalidParameterException("No tag with this name exists");
        }


        book.Tags.Remove(tag);
        
        await _bookRepository.SaveChangesAsync();
    }

    public async Task DeleteBooksAsync(string email, IEnumerable<string> bookTitles)
    {
        var user = await CheckIfUserExistsAsync(email, trackChanges: true);

        foreach (var bookTitle in bookTitles)
        {
            var book = GetBookIfExists(user, bookTitle);
            _bookRepository.DeleteBook(book);
        }

        await _bookRepository.SaveChangesAsync();
    }

    private Book GetBookIfExists(User user, string bookTitle)
    {
        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        if (book == null)
        {
            throw new InvalidParameterException("No book with the title \"" + bookTitle + "\" found");
        }

        return book;
    }

    private async Task<User> CheckIfUserExistsAsync(string email, bool trackChanges)
    {
        var user = await _userRepository.GetAsync(email, trackChanges);
        if (user == null)
        {
            throw new InvalidParameterException("No user with this email exists");
        }

        return user;
    }
}