using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookOutDto
{
    public string Title { get; set; }
    
    public DateTime ReleaseDate { get; set; }
    
    public int Pages { get; set; }
    
    public string Format { get; set; }

    public int CurrentPage { get; set; }
    
    public DateTime CreationDate { get; set; }

    public DateTime LastOpened { get; set; }
    
    
    
    public ICollection<AuthorOutDto> Authors { get; set; }
    
    public ICollection<TagOutDto> Tags { get; set; }
}