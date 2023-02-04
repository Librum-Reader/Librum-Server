using Application.Common.DTOs.Books;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services.v1;

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
        book.BookId = bookInDto.Guid;

        if (await _bookRepository.ExistsAsync(user.Id, book.BookId))
        {
            const string message = "A book with this guid already exists";
            throw new InvalidParameterException(message);
        }

        // Add tags
        foreach (var tag in bookInDto.Tags)
        {
            var newTag = user.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
            if (newTag == default)
            {
                newTag = _mapper.Map<Tag>(tag);
                newTag.UserId = user.Id;
            }
            
            book.Tags.Add(newTag);
        }
        
        user.Books.Add(book);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task<IList<BookOutDto>> GetBooksAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);

        var books = _bookRepository.GetAllAsync(user.Id);
        await _bookRepository.LoadRelationShipsAsync(books);

        return books.Select(book => _mapper.Map<BookOutDto>(book)).ToList();
    }

    public async Task DeleteBooksAsync(string email, IEnumerable<Guid> bookGuids)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        foreach (var bookGuid in bookGuids)
        {
            var book = user.Books.SingleOrDefault(book => book.BookId == bookGuid);
            if (book == default)
            {
                const string message = "No book with this guid exists";
                throw new InvalidParameterException(message);
            }

            await _bookRepository.LoadRelationShipsAsync(book);
            _bookRepository.DeleteBook(book);
        }

        await _bookRepository.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(string email,
                                     BookForUpdateDto bookUpdateDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == bookUpdateDto.Guid);
        if (book == null)
        {
            const string message = "No book with this guid exists";
            throw new InvalidParameterException(message);
        }
        await _bookRepository.LoadRelationShipsAsync(book);
        

        var bookProperties = bookUpdateDto.GetType().GetProperties();
        foreach (var property in bookProperties)
        {
            if(property.Name.Equals("Guid"))
                continue;
            
            if (property.Name.Equals("Tags"))
            {
                MergeTags(bookUpdateDto.Tags, book, user);
                continue;
            }

            var value = property.GetValue(bookUpdateDto);
            if (value is null || (value is int i && i == 0))
                continue;

            var bookProperty = book.GetType().GetProperty(property.Name);
            if (bookProperty == null)
            {
                var message = "The book class does not contain a property" +
                              " called: " + property.Name;
                throw new InvalidParameterException(message);
            }

            bookProperty.SetValue(book, value);
        }

        await _bookRepository.SaveChangesAsync();
    }

    private void MergeTags(ICollection<TagInDto> tags, Book book, User user)
    {
        // Delete all tags which no longer exist
        foreach (var tag in book.Tags)
        {
            if (tags.All(t => t.Guid != tag.TagId))
                book.Tags.Remove(tag);
        }

        // Add tags
        foreach (var tag in tags)
        {
            // Skip if already has tag
            var existingTag = book.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
            if (existingTag != default)
            {
                existingTag.Name = tag.Name;
                continue;
            }

            // Create new tag
            var newTag = user.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
            if (newTag == default)
            {
                newTag = _mapper.Map<Tag>(tag);
                newTag.UserId = user.Id;
            }
            
            book.Tags.Add(newTag);
        }
    }
}