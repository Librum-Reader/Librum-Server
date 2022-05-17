using Application.Common.DTOs.Books;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.RequestParameters;
using AutoMapper;
using Domain.Entities;

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
        
        var queryMatchingBooks = await _bookRepository.GetBooksByQuery(user.Id, bookRequestParameter.Query,
            bookRequestParameter.PageNumber, bookRequestParameter.PageSize);

        var sortedBooks = SortBooks(queryMatchingBooks, bookRequestParameter);
        
        
        return sortedBooks.Select(book => _mapper.Map<BookOutDto>(book)).ToList();
    }

    private IList<Book> SortBooks(IList<Book> books, BookRequestParameter bookRequestParameter)
    {
        switch (bookRequestParameter.SortBy)
        {
            case BookSortOptions.RecentlyRead:
                return books.OrderByDescending((x) => x.LastOpened).ToList();
            case BookSortOptions.RecentlyAdded:
                return books.OrderByDescending((x) => x.CreationDate).ToList();
            default:
                return books;
        }
    }
}