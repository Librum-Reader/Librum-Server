using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.RequestParameters;
using Application.Extensions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        var book = _mapper.Map<Book>(bookInDto);
        user.Books.Add(book);

        await _bookRepository.SaveChangesAsync();
    }

    public async Task<IList<BookOutDto>> GetBooksAsync(string email, BookRequestParameter bookRequestParameter)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);

        var books = _bookRepository.GetAllAsync(user.Id);
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
        
        return await processedBooks.Select(book => _mapper.Map<BookOutDto>(book)).ToListAsync();
    }

    public async Task AddTagsToBookAsync(string email, string bookTitle, IEnumerable<string> tagNames)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var book = user.Books.Single(book => book.Title == bookTitle);
        await _bookRepository.LoadRelationShipsAsync(book);
        
        foreach (var tagName in tagNames)
        {
            var tag = GetTagIfDoesNotExist(user, tagName);
            book.Tags.Add(tag);
        }

        await _bookRepository.SaveChangesAsync();
    }

    private static Tag GetTagIfDoesNotExist(User user, string tagName)
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
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var book = user.Books.Single(book => book.Title == bookTitle);
        await _bookRepository.LoadRelationShipsAsync(book);

        var tag = book.Tags.SingleOrDefault(tag => tag.Name == tagName);

        book.Tags.Remove(tag);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task DeleteBooksAsync(string email, IEnumerable<string> bookTitles)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        foreach (var bookTitle in bookTitles)
        {
            var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
            if (book == null)
            {
                throw new InvalidParameterException("No book with this title exists");
            }
            await _bookRepository.LoadRelationShipsAsync(book);
            
            _bookRepository.DeleteBook(book);
        }

        await _bookRepository.SaveChangesAsync();
    }
    
    public async Task PatchBookAsync(string email, JsonPatchDocument<BookForUpdateDto> patchDoc, string bookTitle,
        ControllerBase controllerBase)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.Single(book => book.Title == bookTitle);

        var bookToPatch = _mapper.Map<BookForUpdateDto>(book);
        

        patchDoc.ApplyTo(bookToPatch, controllerBase.ModelState);
        controllerBase.TryValidateModel(controllerBase.ModelState);

        if (!controllerBase.ModelState.IsValid || !bookToPatch.DataIsValid)
        {
            throw new InvalidParameterException("The provided data is invalid");
        }

        _mapper.Map(bookToPatch, book);

        await _bookRepository.SaveChangesAsync();
    }

    public async Task AddAuthorToBookAsync(string email, string bookTitle, AuthorInDto authorToAdd)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var book = user.Books.Single(book => book.Title == bookTitle);
        await _bookRepository.LoadRelationShipsAsync(book);

        if (book!.Authors.Any(author =>
                author.FirstName == authorToAdd.FirstName && author.LastName == authorToAdd.LastName))
        {
            throw new InvalidParameterException("An author with this name already exists");
        }

        var author = _mapper.Map<Author>(authorToAdd);
        book.Authors.Add(author);
        
        await _bookRepository.SaveChangesAsync();
    }

    public async Task RemoveAuthorFromBookAsync(string email, string bookTitle, AuthorForRemovalDto authorToRemove)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var book = user.Books.SingleOrDefault(book => book.Title == bookTitle);
        await _bookRepository.LoadRelationShipsAsync(book);

        var author = book!.Authors.SingleOrDefault(author => 
            author.FirstName == authorToRemove.FirstName && author.LastName == authorToRemove.LastName);
        if (author == null)
        {
            throw new InvalidParameterException("No author with this name exists");
        }
        
        book.Authors.Remove(author);
        await _bookRepository.SaveChangesAsync();
    }
}