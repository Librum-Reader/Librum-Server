using System.Globalization;
using Application.Common.DTOs.Books;
using Application.Common.DTOs.Users;
using Application.Interfaces.Services;

namespace Infrastructure.Persistence;

public static class DataContextSeeding
{
    public static async Task SeedDataContext(DataContext context,
                                             IAuthenticationService authenticationService,
                                             IBookService bookService)
    {
        if (context.Users.Any())
            return;


        var users = new List<RegisterDto>
        {
            new RegisterDto
            {
                FirstName = "Luke",
                LastName = "Ratatui",
                Email = "LukeRatatui@gmail.com",
                Password = "MyPassword123"
            },
            new RegisterDto
            {
                FirstName = "Lisa",
                LastName = "Lambatz",
                Email = "LisaLambatz@gmail.com",
                Password = "MyPassword123"
            },
            new RegisterDto
            {
                FirstName = "Kaktor",
                LastName = "Dumbatz",
                Email = "KaktorDumbatz@gmail.com",
                Password = "MyPassword123"
            }
        };

        var books = new List<BookInDto>()
        {
            new BookInDto
            {
                Guid = Guid.NewGuid().ToString(),
                Title = "LukesRandomBook",
                PageCount = 1200,
                CurrentPage = 2,
                Format = "Pdf",
                DocumentSize = "2MiB",
                PagesSize = "800 x 300",
                CreationDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                AddedToLibrary = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                LastModified = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            },
            new BookInDto
            {
                Guid = Guid.NewGuid().ToString(),
                Title = "LisasRandomBook",
                PageCount = 409,
                CurrentPage = 211,
                Format = "Mobi",
                DocumentSize = "0.7MiB",
                PagesSize = "Mostly 200 x 800",
                CreationDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                AddedToLibrary = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                LastModified = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            },
            new BookInDto
            {
                Guid = Guid.NewGuid().ToString(),
                Title = "KaktorsRandomBook",
                PageCount = 931200,
                Format = "Epub",
                CurrentPage = 1234,
                DocumentSize = "0.7MiB",
                PagesSize = "Mostly 200 x 800",
                CreationDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                AddedToLibrary = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                LastModified = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            }
        };


        for (int i = 0; i < users.Count; ++i)
        {
            await authenticationService.RegisterUserAsync(users[i]);
            await bookService.CreateBookAsync(users[i].Email, books[i],
                                              Guid.NewGuid().ToString());
        }
    }
}