using Application.Common.DTOs.Books;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
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


    public async Task CreateBook(string email, BookInDto bookInDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (user == null)
            throw new InvalidParameterException("No user with the given email exists");
        
        var book = _mapper.Map<Book>(bookInDto);

        await _userRepository.LoadRelationShips(user);
        user.Books.Add(book);

        await _bookRepository.SaveChangesAsync();
    }
}