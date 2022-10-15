using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookOutDto
{
    public string Guid { get; set; }
    
    public string Title { get; set; }
    
    public int Pages { get; set; }
    
    public int CurrentPage { get; set; }
    
    public string Format { get; set; }
    
    public string Language { get; set; }
    
    public string DocumentSize { get; set; }

    public string PagesSize { get; set; }
    
    public string Creator { get; set; }
    
    public string DataLink { get; set; }
    
    public string CoverLink { get; set; }
    
    public string ReleaseDate { get; set; }
    
    public string AddedToLibrary { get; set; }

    public string LastOpened { get; set; }
    
    
    
    public ICollection<AuthorOutDto> Authors { get; set; }
    
    public ICollection<TagOutDto> Tags { get; set; }
}