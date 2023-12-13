using Application.Common.DTOs.Bookmarks;
using Application.Common.DTOs.Highlights;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookOutDto
{
    public string Guid { get; set; }
    
    public string Title { get; set; }
    
    public int PageCount { get; set; }
    
    public int CurrentPage { get; set; }
    
    public string Format { get; set; }
    
    public string Language { get; set; }
    
    public string DocumentSize { get; set; }

    public string PagesSize { get; set; }
    
    public string Creator { get; set; }
    
    public string Authors { get; set; }
    
    public string CreationDate { get; set; }
    
    public string AddedToLibrary { get; set; }

    public string LastOpened { get; set; }
    
    public string LastModified { get; set; }
    
    public string CoverLastModified { get; set; }
    
    public bool HasCover { get; set; }
    
    public int ProjectGutenbergId { get; set; }
    
    public string ColorTheme { get; set; }
    
    public string FileHash { get; set; }

    public ICollection<TagOutDto> Tags { get; set; } = new List<TagOutDto>();
    
    public ICollection<HighlightOutDto> Highlights { get; set; } = new List<HighlightOutDto>();
    
    public ICollection<BookmarkOutDto> Bookmarks { get; set; } = new List<BookmarkOutDto>();
}