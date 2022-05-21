using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Books;
using Application.Common.DTOs.Users;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence;

public static class DataContextSeeding
{
    public static async Task SeedDataContext(DataContext context, IAuthenticationService authenticationService, IBookService bookService)
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
                Title = "LukesRandomBook",
                ReleaseDate = DateTime.Now,
                Authors = new List<AuthorInDto>()
                {
                    new AuthorInDto
                    {
                        FirstName = "Dave",
                        LastName = "Lebovski"
                    },
                    new AuthorInDto()
                    {
                        FirstName= "Sos",
                        LastName = "trator"
                    }
                },
                Pages = 1200,
                Format = BookFormats.Pdf,
                CurrentPage = 2
            },
            new BookInDto
            {
                Title = "LisasRandomBook",
                ReleaseDate = DateTime.Now,
                Authors = new List<AuthorInDto>()
                {
                    new AuthorInDto
                    {
                        FirstName = "Kai",
                        LastName = "Jeff"
                    }
                },
                Pages = 409,
                Format = BookFormats.Mobi,
                CurrentPage = 211
            },
            new BookInDto
            {
                Title = "KaktorsRandomBook",
                ReleaseDate = DateTime.Now,
                Authors = new List<AuthorInDto>()
                {
                    new AuthorInDto
                    {
                        FirstName = "Vorel",
                        LastName = "Nameskin"
                    }
                },
                Pages = 931200,
                Format = BookFormats.Epub,
                CurrentPage = 1234
            }
        };


        for (int i = 0; i < users.Count; ++i)
        {
            await authenticationService.RegisterUserAsync(users[i]);
            await bookService.CreateBookAsync(users[i].Email, books[i]);
        }
    }
}