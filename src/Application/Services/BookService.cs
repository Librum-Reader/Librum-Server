using Application.Common.DTOs.Bookmarks;
using Application.Common.DTOs.Books;
using Application.Common.DTOs.Tags;
using Application.Common.DTOs.Highlights;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class BookService : IBookService
{
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IBookBlobStorageManager _bookBlobStorageManager;

    public BookService(IMapper mapper, IBookRepository bookRepository,
                       IUserRepository userRepository,
                       IConfiguration configuration,
                       IBookBlobStorageManager bookBlobStorageManager)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _bookBlobStorageManager = bookBlobStorageManager;
    }


    public async Task CreateBookAsync(string email, BookInDto bookInDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        if (await _bookRepository.ExistsAsync(user.Id, bookInDto.Guid))
        {
            const string message = "A book with this id already exists";
            throw new CommonErrorException(400, message, 0);
        }

        if (_configuration["LIBRUM_SELFHOSTED"] != "true" && !await UserHasEnoughStorageSpaceAvailable(user))
        {
            const string message = "Book storage space is insufficient";
            throw new CommonErrorException(426, message, 5);
        }

        var book = _mapper.Map<Book>(bookInDto);
        book.BookId = bookInDto.Guid;

        foreach (var tag in bookInDto.Tags)
        {
            AddTagDtoToBook(book, tag, user);
        }

        foreach (var highlight in bookInDto.Highlights)
        {
            var newHighlight = _mapper.Map<Highlight>(highlight);
            newHighlight.BookId = book.BookId;
        
            book.Highlights.Add(newHighlight);
        }

        foreach (var bookmark in bookInDto.Bookmarks)
        {
            var newBookmark = _mapper.Map<Bookmark>(bookmark);
            newBookmark.BookId = book.BookId;

            book.Bookmarks.Add(newBookmark);
        }

        user.Books.Add(book);
        await _bookRepository.SaveChangesAsync();
    }

    private async Task<bool> UserHasEnoughStorageSpaceAvailable(User user)
    {
        var usedStorage = await _bookRepository.GetUsedBookStorage(user.Id);
        return usedStorage <= user.BookStorageLimit;
    }

    public async Task<IList<BookOutDto>> GetBooksAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        var books = _bookRepository.GetAllAsync(user.Id, true).ToList();
        
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
                const string message = "No book with this id exists";
                throw new CommonErrorException(404, message, 4);
            }

            await _bookRepository.LoadRelationShipsAsync(book);
            _bookRepository.DeleteBook(book);

            // When a book is deleted while media file was not yet uploaded, the
            // delete should still succeed.
            try
            {
                await _bookBlobStorageManager.DeleteBookBlob(book.BookId);
            }
            catch (Exception)
            {
                // ignored
            }

            if (book.HasCover)
            {
                try
                {
                    await _bookBlobStorageManager.DeleteBookCover(book.BookId);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        await _bookRepository.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(string email, BookForUpdateDto bookUpdateDto)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == bookUpdateDto.Guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }
        await _bookRepository.LoadRelationShipsAsync(book);

        var dtoProperties = bookUpdateDto.GetType().GetProperties();
        foreach (var dtoProperty in dtoProperties)
        {
            // Manually handle certain properties
            switch (dtoProperty.Name)
            {
                case "Guid":
                    continue;     // Can't modify the GUID
                case "Tags":
                    MergeTags(bookUpdateDto.Tags, book, user);
                    continue;
                case "Highlights":
                    MergeHighlights(bookUpdateDto.Highlights, book);
		    continue;
                case "Bookmarks":
                    MergeBookmarks(bookUpdateDto.Bookmarks, book);
                    continue;
            }
            
            // Update any other property via reflection
            var value = dtoProperty.GetValue(bookUpdateDto);
            SetPropertyOnBook(book, dtoProperty.Name, value);
        }

        await _bookRepository.SaveChangesAsync();
    }

    public async Task AddBookBinaryData(string email, Guid guid, MultipartReader reader)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        try
        {
            await _bookBlobStorageManager.UploadBookBlob(guid, reader);
        }
        catch (Exception _)
        {
            // If uploading the book's data fails, make sure to remove the book
            // from the SQL Database, so that no invalid book exist
            await _bookRepository.LoadRelationShipsAsync(book);
            _bookRepository.DeleteBook(book);
            await _bookRepository.SaveChangesAsync();

            throw;
        }
    }

    public async Task<Stream> GetBookBinaryData(string email, Guid guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        return await _bookBlobStorageManager.DownloadBookBlob(guid);
    }

    public async Task<Stream> GetBookCover(string email, Guid guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        return await _bookBlobStorageManager.DownloadBookCover(guid);
    }

    public async Task DeleteBookCover(string email, Guid guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        await _bookBlobStorageManager.DeleteBookCover(guid);
    }

    public async Task<string> GetFormatForBook(string email, Guid guid)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        return book.Format;
    }

    public async Task ChangeBookCover(string email, Guid guid, MultipartReader reader)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var book = user.Books.SingleOrDefault(book => book.BookId == guid);
        if (book == null)
        {
            const string message = "No book with this id exists";
            throw new CommonErrorException(404, message, 4);
        }

        var coverSize = await _bookBlobStorageManager.ChangeBookCover(guid, reader);
        book.CoverSize = coverSize;

        await _bookRepository.SaveChangesAsync();
    }
    
    private void SetPropertyOnBook(Book book, string property, object value)
    {
        var bookProperty = book.GetType().GetProperty(property);
        if (bookProperty == null)
        {
            var message = "Book has no property called: " + property;
            throw new CommonErrorException(400, message, 0);
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
            
            AddTagDtoToBook(book, tag, user);
        }
    }
    
    private void AddTagDtoToBook(Book book, TagInDto tag, User user)
    {
        // Return if the book already owns the tag
        if (book.Tags.SingleOrDefault(t => t.TagId == tag.Guid) != null)
            return;
        
        // If the tag already exists, just add it to the book
        var existingTag = user.Tags.SingleOrDefault(t => t.TagId == tag.Guid);
        if (existingTag != null)
        {
            book.Tags.Add(existingTag);
            return;
        }
        
        // If the book already has a tag with the same name, throw
        if (book.Tags.Any(t => t.Name == tag.Name))
        {
            var message = "A tag with this name already exists";
            throw new CommonErrorException(400, message, 6);
        }
        
        // Create the tag from scratch
        var newTag = _mapper.Map<Tag>(tag);
        newTag.UserId = user.Id;
        book.Tags.Add(newTag);
    }
    
    /// When a book is updated, a list of tags is sent with it. This list of tags
    /// is the "source of truth" and contains all tags that the book owns.
    /// If the database book contains tags that the updated list of tags does not
    /// contain, those old tags shall be deleted
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
    
    private void MergeHighlights(ICollection<HighlightInDto> newHighlights, Book book)
    {
        RemoveHighlightsWhichDontExistInNewHighlights(book, newHighlights);
        
        foreach (var newHighlight in newHighlights)
        {
            // If book already has the highlight, update it
            var existingHighlight = book.Highlights.SingleOrDefault(h => h.HighlightId == newHighlight.Guid);
            if (existingHighlight != null)
            {
                if (existingHighlight.Color != newHighlight.Color)
                    existingHighlight.Color = newHighlight.Color;

                continue;
            }
            
            // Else add the highlight to book
            var highlight = _mapper.Map<Highlight>(newHighlight);
            highlight.HighlightId = highlight.HighlightId;
            book.Highlights.Add(highlight);
        }
    }
    
    /// When a book is updated, a list of highlights is sent with it. This list of highlights
    /// is the "source of truth" and contains all highlights that the book owns.
    /// If the database book contains highlights that the updated list of highlights
    /// does not contain, those old highlights shall be deleted.
    private void RemoveHighlightsWhichDontExistInNewHighlights(Book book, 
                                                               ICollection<HighlightInDto> newHighlights)
    {
        var highlightsToRemove = new List<Highlight>();
        foreach (var highlight in book.Highlights)
        {
            if (newHighlights.All(h => h.Guid != highlight.HighlightId))
                highlightsToRemove.Add(highlight);
        }

        foreach (var highlight in highlightsToRemove) { book.Highlights.Remove(highlight); }
    }
    
    private void MergeBookmarks(ICollection<BookmarkInDto> newBookmarks, Book book)
    {
        RemoveBookmarksWhichDontExistInNewBookmarks(book, newBookmarks);
        
        foreach (var newBookmark in newBookmarks)
        {
            // If book already has the bookmark, update it
            var existingBookmark = book.Bookmarks.SingleOrDefault(t => t.BookmarkId == newBookmark.Guid);
            if (existingBookmark != null)
            {
                if (existingBookmark.Name != newBookmark.Name)
                    existingBookmark.Name = newBookmark.Name;

                continue;
            }
            
            // Else add the bookmark to book
            var bookmark = _mapper.Map<Bookmark>(newBookmark);
            bookmark.BookId = book.BookId;
            book.Bookmarks.Add(bookmark);
        }
    }
    
    /// When a book is updated, a list of bookmarks is sent with it. This list of bookmarks
    /// is the "source of truth" and contains all bookmarks that the book owns.
    /// If the database book contains bookmarks that the updated list of bookmarks
    /// does not contain, those old bookmarks shall be deleted.
    private void RemoveBookmarksWhichDontExistInNewBookmarks(Book book, 
                                                             ICollection<BookmarkInDto> newBookmarks)
    {
        var bookmarksToRemove = new List<Bookmark>();
        foreach (var bookmark in book.Bookmarks)
        {
            if (newBookmarks.All(t => t.Guid != bookmark.BookmarkId))
                bookmarksToRemove.Add(bookmark);
        }

        foreach (var bookmark in bookmarksToRemove) { book.Bookmarks.Remove(bookmark); }
    }
}
