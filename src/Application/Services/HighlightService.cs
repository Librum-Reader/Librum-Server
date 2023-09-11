using Application.Common.DTOs.Highlights;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class HighlightService : IHighlightService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHighlightRepository _highlightRepository;
    private readonly IBookRepository _bookRepository;


    public HighlightService(IMapper mapper,
                            IHighlightRepository highlightRepository, 
                            IBookRepository bookRepository,
                            IUserRepository userRepository)
    {
        _mapper = mapper;
        _highlightRepository = highlightRepository;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
    }
    
    
    public async Task CreateHighlightAsync(string email, Guid bookGuid, HighlightInDto highlightIn)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == bookGuid);
        if (book == default)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }
        
        var highlightExists = book.Highlights.Any(h => h.HighlightId == highlightIn.Guid);
        if(highlightExists)
        {
            const string message = "A highlight with this id already exists";
            throw new CommonErrorException(409, message, 0);
        }

        var newHighlight = _mapper.Map<Highlight>(highlightIn);
        newHighlight.Book = book;
        book.Highlights.Add(newHighlight);
        
        await _highlightRepository.SaveChangesAsync();
    }

    public async Task DeleteHighlightAsync(string email, Guid bookGuid, Guid highlightGuid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var bookExists = await _bookRepository.ExistsAsync(user.Id, bookGuid);
        if (!bookExists)
            return;
        
        var highlight = await _highlightRepository.GetAsync(bookGuid, 
                                                            highlightGuid);
        if (highlight == default)
        {
            const string message = "No highlight with this id exists";
            throw new CommonErrorException(404, message, 0);
        }
        
        _highlightRepository.Delete(highlight);
        await _highlightRepository.SaveChangesAsync();
    }
}