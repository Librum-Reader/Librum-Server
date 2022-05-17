using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.Common.RequestParameters;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<Book> SortByBestMatch(this IQueryable<Book> books, string target)
    {
        if (target == string.Empty)
        {
            return books;
        }

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

    public static IQueryable<Book> FilterByAuthor(this IQueryable<Book> books, string authorName)
    {
        if (authorName == string.Empty)
        {
            return books;
        }

        return books.Where(book => book.Authors
            .Any(author => (author.FirstName.ToLower() + " " + author.LastName.ToLower()).Contains(authorName)));
    }

    public static IQueryable<Book> FilterByTimeSinceAdded(this IQueryable<Book> books, TimeSpan timePassed)
    {
        if (timePassed == default)
        {
            return books;
        }

        DateTime lastAcceptedTime = DateTime.Now.Subtract(timePassed);
        return books
            .Where(book => lastAcceptedTime <= book.CreationDate);
    }

    public static IQueryable<Book> FilterByFormat(this IQueryable<Book> books, BookFormats format)
    {
        if (format == default)
        {
            return books;
        }

        return books.Where(book => book.Format == format);
    }

    public static IQueryable<Book> FilterByOptions(this IQueryable<Book> books, BookRequestParameter bookRequestParameter)
    {
        if (bookRequestParameter.Read)
            books = books.Where(book => book.CurrentPage == book.Pages);

        if (bookRequestParameter.Unread)
            books = books.Where(book => book.CurrentPage != book.Pages);
        
        return books;
    }

    public static IQueryable<Book> FilterByTags(this IQueryable<Book> books, string tag)
    {
        // TODO: Implement
        return books;
    }

    public static IQueryable<Book> PaginateBooks(this IQueryable<Book> books, int pageNumber, int pageSize)
    {
        return books
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public static IQueryable<Book> SortByCategories(this IQueryable<Book> books, BookSortOptions sortOption)
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