using Domain.Entities;

namespace Infrastructure.Persistence;

public static class DataContextSeeding
{
    public static async Task SeedDataContext(DataContext context)
    {
        if (context.Users.Any())
            return;


        var users = new List<User>
        {
            new User
            {
                FirstName = "Luke",
                LastName = "Ratatui",
                Email = "LukeRatatui@gmail.com",
                UserName = "LukeRatatui@gmail.com",
                AccountCreation = DateTime.UtcNow
            },
            new User
            {
                FirstName = "Lisa",
                LastName = "Lambatz",
                Email = "LisaLambatz",
                UserName = "LisaLambatz",
                AccountCreation = DateTime.UtcNow,
                Books = new List<Book>
                {
                    new Book
                    {
                        Title = "Professional CMake",
                        ReleaseDate = new DateTime(2018, 01, 02),
                        Format = "Pdf",
                        Pages = 1101,
                        Authors = new List<Author>
                        {
                            new Author
                            {
                                FirstName = "Lerry",
                                LastName = "Wheelson"
                            },
                            new Author
                            {
                                FirstName = "Major",
                                LastName = "Tom"
                            }
                        }
                    },
                    new Book
                    {
                        Title = "Test Driven Development with C++",
                        ReleaseDate = new DateTime(2012, 02, 27),
                        Format = "Pdf",
                        Pages = 521,
                        Authors = new List<Author>
                        {
                            new Author
                            {
                                FirstName = "Alfred",
                                LastName = "Orus"
                            }
                        }
                    },
                    new Book
                    {
                        Title = "C# in a nutshell",
                        ReleaseDate = new DateTime(2019, 11, 14),
                        Format = "Pdf",
                        Pages = 1243
                    }
                }
            },
            new User
            {
                FirstName = "Kaktor",
                LastName = "Dumbatz",
                Email = "KaktorDumbatz@gmail.com",
                UserName = "KaktorDumbatz@gmail.com",
                AccountCreation = DateTime.UtcNow,
                Books = new List<Book>
                {
                    new Book
                    {
                        Title = "Im ok; your ok",
                        ReleaseDate = new DateTime(2009, 07, 21),
                        Format = "Epub",
                        Pages = 412
                    },
                    new Book
                    {
                        Title = "Professional CMake",
                        ReleaseDate = new DateTime(2018, 01, 02),
                        Format = "Pdf",
                        Pages = 1101,
                        Authors = new List<Author>
                        {
                            new Author
                            {
                                FirstName = "Lerry",
                                LastName = "Wheelson"
                            },
                            new Author
                            {
                                FirstName = "Major",
                                LastName = "Tom"
                            }
                        }
                    }
                }
            }
        };

        context.AddRange(users);
        await context.SaveChangesAsync();
    }
}