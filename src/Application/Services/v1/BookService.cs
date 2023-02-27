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
        if (await _bookRepository.ExistsAsync(user.Id, bookInDto.Guid))
        {
            const string message = "A book with this guid already exists";
            throw new InvalidParameterException(message);
        }
        
        var book = _mapper.Map<Book>(bookInDto);
        book.BookId = bookInDto.Guid;
        AddTagDtosToBook(bookInDto.Tags, book, user);

        user.Books.Add(book);
        await _bookRepository.SaveChangesAsync();
    }

    private void AddTagDtosToBook(IEnumerable<TagInDto> tags, Book book, User user)
    {
        foreach (var tag in tags)
        {
            // If the tag already exists just add it to the book
            var existingTag = user.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
            if (existingTag != null)
            {
                book.Tags.Add(existingTag);
                continue;
            }

            var newTag = _mapper.Map<Tag>(tag);
            newTag.UserId = user.Id;
            book.Tags.Add(newTag);
        }
    }

    public async Task<IList<BookOutDto>> GetBooksAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);

        var books = _bookRepository.GetAllAsync(user.Id).ToList();
        await _bookRepository.LoadRelationShipsAsync(books);

        return books.Select(book => _mapper.Map<BookOutDto>(book)).ToList();
    }

    public async Task DeleteBooksAsync(string email, IEnumerable<Guid> bookGuids)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        foreach (var bookGuid in bookGuids)
        {
            var book = user.Books.SingleOrDefault(book => book.BookId == bookGuid);
            if (book == null)
            {
                const string message = "No book with this guid exists";
                throw new InvalidParameterException(message);
            }

            await _bookRepository.LoadRelationShipsAsync(book);
            _bookRepository.DeleteBook(book);
        }

        await _bookRepository.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(string email, BookForUpdateDto bookUpdateDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == bookUpdateDto.Guid);
        if (book == null)
        {
            const string message = "No book with this guid exists";
            throw new InvalidParameterException(message);
        }
        await _bookRepository.LoadRelationShipsAsync(book);
        

        var dtoProperties = bookUpdateDto.GetType().GetProperties();
        foreach (var dtoProperty in dtoProperties)
        {
            // Manually handle certain properties
            switch (dtoProperty.Name)
            {
                case "Guid":
                    continue;
                case "Tags":
                    MergeTags(bookUpdateDto.Tags, book, user);
                    continue;
                case "CoverLink": // TODO: Implement CoverLink
                    continue;
            }
            
            // Update the book value
            var value = dtoProperty.GetValue(bookUpdateDto);
            SetPropertyOnBook(book, dtoProperty.Name, value);
        }

        await _bookRepository.SaveChangesAsync();
    }

    private void SetPropertyOnBook(Book book, string property, object value)
    {
        var bookProperty = book.GetType().GetProperty(property);
        if (bookProperty == null)
        {
            var message = "The book class does not contain a property" +
                          " called: " + property;
            throw new InvalidParameterException(message);
        }
        
        bookProperty.SetValue(book, value);
    }

    private void MergeTags(ICollection<TagInDto> newTags, Book book, User user)
    {
        RemoveBookTagsWhichDontExistInNewTags(book, newTags);
        
        foreach (var tag in newTags)
        {
            // If book already has the tag, update it
            var existingTag = book.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
            if (existingTag != null)
            {
                existingTag.Name = tag.Name;
                continue;
            }

            if (book.Tags.Any(t => t.Name == tag.Name))
            {
                var message = "A tag with this name already exists";
                throw new InvalidParameterException(message);
            }

            AddTagDtoToBook(book, tag, user);
        }
    }
    
    private void RemoveBookTagsWhichDontExistInNewTags(Book book, 
                                                       ICollection<TagInDto> newTags)
    {
        var tagsToRemove = new List<Tag>();
        foreach (var tag in book.Tags)
        {
            if (newTags.All(t => t.Guid != tag.TagId))
                tagsToRemove.Add(tag);
        }

        foreach (var tag in tagsToRemove) {  book.Tags.Remove(tag); }
    }

    private void AddTagDtoToBook(Book book, TagInDto tag, User user)
    {
        var existingTag = user.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
        if (existingTag != null)
        {
            book.Tags.Add(existingTag);
            return;
        }
        
        var newTag = _mapper.Map<Tag>(tag);
        newTag.UserId = user.Id;
        book.Tags.Add(newTag);
    }
}