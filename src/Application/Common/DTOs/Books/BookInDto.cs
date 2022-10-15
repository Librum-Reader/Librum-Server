using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Authors;
using Domain.Enums;

namespace Application.Common.DTOs.Books;

public class BookInDto
{
    [Required]
    public string Guid { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The provided title is too short")]
    [MaxLength(80, ErrorMessage = "The provided title is too long")]
    public string Title { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Pages { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    [Required]
    public BookFormat Format { get; set; }

    public string Language { get; set; }

    [Required]
    public string DocumentSize { get; set; }

    [Required]
    public string PagesSize { get; set; }
    
    public string Creator { get; set; }
    
    [Required]
    public string ReleaseDate { get; set; }
    
    [Required]
    public string AddedToLibrary { get; set; }
    
    public string LastOpened { get; set; }
    
    public string Cover { get; set; }
    
    public string Data { get; set; }
    
    public IList<AuthorInDto> Authors { get; set; }

    
    public bool IsValid => CurrentPage <= Pages;
}