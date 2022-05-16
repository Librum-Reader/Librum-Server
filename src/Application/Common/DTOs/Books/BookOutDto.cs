using Application.Common.DTOs.Authors;

namespace Application.Common.DTOs.Books;

public class BookOutDto
{
    public string Title { get; set; }
    
    public DateTime ReleaseDate { get; set; }
    
    public int Pages { get; set; }
    
    public string Format { get; set; }

    public int CurrentPage { get; set; }
    
    public IList<AuthorOutDto> Authors { get; set; }
}