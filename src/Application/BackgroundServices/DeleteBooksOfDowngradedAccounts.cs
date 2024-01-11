using System.Globalization;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.BackgroundServices;

public class DeleteBooksOfDowngradedAccounts(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider
                    .GetService<ILogger<DeleteUnconfirmedUsers>>();
                logger.LogWarning("Deleting books of downgraded accounts");

                var userRepository = scope.ServiceProvider.GetService<IUserRepository>();
                var downgradedUsers = await userRepository.GetUsersWhoDowngradedMoreThanAWeekAgo();

                var bookService = scope.ServiceProvider.GetService<IBookService>();
                var bookRepository = scope.ServiceProvider.GetService<IBookRepository>();
                var productRepository = scope.ServiceProvider.GetService<IProductRepository>();

                foreach (var user in downgradedUsers)
                {
                    var product = await productRepository.GetAll().FirstOrDefaultAsync(p => p.ProductId == user.ProductId);
                    if (product == null)
                    {
                        logger.LogWarning($"User with email {user.Email} has an invalid product id");
                        continue;
                    }
                    
                    var allowedBookStorage = product.BookStorageLimit;
                    var usedBookStorage = await bookRepository.GetUsedBookStorage(user.Id);
                    long difference = usedBookStorage - allowedBookStorage;
                    if (difference <= 0)
                        continue;
                    
                    var books = await bookRepository.GetAllAsync(user.Id, loadRelationships: true).ToListAsync();
                    SortBooksByAddedToLibrary(books);
                    
                    // We allow adding one more book as long as the currently used storage is less than the max storage,
                    // so we need to check if the difference is less than the size of the latest book.
                    if (difference <= (long)GetBytesFromSizeString(books.First().DocumentSize))
                        continue;
                    
                    logger.LogWarning($"User with email {user.Email} has exceeded their book storage limit");
                    
                    await DeleteLatestBooks(books, user.Email, difference, bookService);
                    
                    // We have already dealt with them, avoid checking them again unless their tier changes.
                    var trackingUser = await userRepository.GetAsync(user.Email, trackChanges: true);
                    trackingUser.AccountLastDowngraded = DateTime.MaxValue;
                }
            }
            
            // Repeat the check every 12 hours.
            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
    
    private void SortBooksByAddedToLibrary(List<Book> books)
    {
        var ci = new CultureInfo("de-DE");
        books.Sort((b1, b2) =>
        {
            var b1Dt = DateTime.ParseExact(b1.AddedToLibrary, "HH:mm:ss - dd.MM.yyyy", ci);
            var b2Dt = DateTime.ParseExact(b2.AddedToLibrary, "HH:mm:ss - dd.MM.yyyy", ci);
            return b1Dt.CompareTo(b2Dt);
        });
        
        books.Reverse();
    }
    
    private async Task DeleteLatestBooks(List<Book> books, string email, long difference, IBookService bookService)
    {
        List<Guid> booksToDelete = new();
        while (difference - (long)GetBytesFromSizeString(books.First().DocumentSize) > 0)
        {
            var book = books.First();
            booksToDelete.Add(book.BookId);
            difference -= (long)GetBytesFromSizeString(book.DocumentSize);
            books.Remove(book);
        }

        await bookService.DeleteBooksAsync(email, booksToDelete);
    }
    
    private double GetBytesFromSizeString(string size)
    {
        size = size.Replace(" ", string.Empty);
        size = size.Replace(",", ".");
        
        int typeBegining = -1;
        for (int i = 0; i < size.Length; i++)
        {
            if (!char.IsDigit(size[i]) && size[i] != '.')
            {
                typeBegining = i;
                break;
            }
        }

        var numberString = size.Substring(0, typeBegining);
        var provider = new System.Globalization.NumberFormatInfo();
        provider.NumberDecimalSeparator = ".";
        provider.NumberGroupSeparator = ",";
        var numbers = Convert.ToDouble(numberString,provider);
		
        var type = size[typeBegining..];
        return type.ToLower() switch
        {
            "b" => numbers,
            "kb" => numbers * 1000,
            "mb" => numbers * 1000 * 1000,
            "gb" => numbers * 1000 * 1000
        };
    }
}