using System.Diagnostics;
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
    private readonly ITagRepository _tagRepository;

    public BookService(IMapper mapper, IBookRepository bookRepository, 
        IUserRepository userRepository, ITagRepository tagRepository)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
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
        

        var result = books
            .FilterByTags(bookRequestParameter.Tag)
            .FilterByAuthor(bookRequestParameter.Author.ToLower())
            .FilterByTimeSinceAdded(bookRequestParameter.TimePassed)
            .FilterByFormat(bookRequestParameter.Format)
            .FilterByOptions(bookRequestParameter)
            .SortByBestMatch(bookRequestParameter.SearchString.ToLower())
            .SortByCategories(bookRequestParameter.SortBy, bookRequestParameter.SearchString)
            .PaginateBooks(bookRequestParameter.PageNumber, bookRequestParameter.PageSize);


        return await result.Select(book => _mapper.Map<BookOutDto>(book)).ToListAsync();
    }

    public async Task AddTagsToBookAsync(string email, string bookTitle, IEnumerable<string> tagNames)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        await _userRepository.LoadRelationShipsAsync(user);
        
        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        if (book == null)
        {
            throw new InvalidParameterException("No book with the given name exists");
        }

        await _bookRepository.LoadRelationShipsAsync(book);
        
        foreach (var tagName in tagNames)
        {
            var tag = user.Tags.SingleOrDefault(tag => tag.Name == tagName);
            if (tag == null)
            {
                throw new InvalidParameterException("No tag called " + tagName + " exists");
            }
            
            book.Tags.Add(tag);
        }

        await _bookRepository.SaveChangesAsync();
    }

    public async Task RemoveTagFromBookAsync(string email, string bookTitle, string tagName)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
        {
            throw new InvalidParameterException("No user with the given email exists");
        }

        await _userRepository.LoadRelationShipsAsync(user);

        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        if (book == null)
        {
            throw new InvalidParameterException("No book with the given title exists");
        }

        await _bookRepository.LoadRelationShipsAsync(book);

        var tag = book.Tags.SingleOrDefault(tag => tag.Name == tagName);
        if (tag == null)
        {
            throw new InvalidParameterException("No tag with the given name exists");
        }


        book.Tags.Remove(tag);
        
        await _bookRepository.SaveChangesAsync();
    }
}