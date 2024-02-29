using System.ComponentModel.DataAnnotations;
using Application.Common.DataAnnotations;
using Application.Common.DTOs.Bookmarks;
using Application.Common.DTOs.Highlights;
using Application.Common.DTOs.Tags;

namespace Application.Common.DTOs.Books;

public class BookForUpdateDto
{
    [Required]
    public Guid Guid { get; set; }

    [MinLength(2, ErrorMessage = "The title is too short")]
    [MaxLength(2000, ErrorMessage = "The title is too long")]
    public string Title { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    [EmptyOrMinLength(2, ErrorMessage = "The language is too short")]
    [MaxLength(100, ErrorMessage = "The language is too long")]
    public string Language { get; set; }

    [MaxLength(2000, ErrorMessage = "The creator is too long")]
    public string Creator { get; set; }

    [MaxLength(2000, ErrorMessage = "The authors are too long")]
    public string Authors { get; set; }

    [MaxLength(100, ErrorMessage = "The creation date is too long")]
    public string CreationDate { get; set; }

    [EmptyOrMinLength(4, ErrorMessage = "The last opened is too short")]
    [MaxLength(100, ErrorMessage = "The last opened is too long")]
    public string LastOpened { get; set; }

    [MinLength(4, ErrorMessage = "The last modified is too short")]
    [MaxLength(100, ErrorMessage = "The last modified is too long")]
    public string LastModified { get; set; }

    [MinLength(4, ErrorMessage = "The cover last modified is too short")]
    [MaxLength(100, ErrorMessage = "The cover last modified is too long")]
    public string CoverLastModified { get; set; }
    
    public bool HasCover { get; set; }
    
    [Required]
    public int ProjectGutenbergId { get; set; }
    
    [Required]
    [MaxLength(100, ErrorMessage = "ColorTheme is too long")]
    public string ColorTheme { get; set; }
    
    [MaxLength(100, ErrorMessage = "ParentFolderId is too long")]
    public string ParentFolderId { get; set; }
    
    public ICollection<TagInDto> Tags { get; set; } = new List<TagInDto>();
    
    public ICollection<HighlightInDto> Highlights { get; set; } = new List<HighlightInDto>();
    
    public ICollection<BookmarkInDto> Bookmarks { get; set; } = new List<BookmarkInDto>();
}