using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Authors;


namespace Application.Common.DTOs.Books;


public class BookInDto
{
    [Required]
    public string Guid { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The title is too short")]
    [MaxLength(80, ErrorMessage = "The title is too long")]
    public string Title { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Pages { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    [Required]
    public string Format { get; set; }

    public string Language { get; set; }

    [Required]
    public string DocumentSize { get; set; }
    
    public string PagesSize { get; set; }
    
    public string Creator { get; set; }
    
    public string CreationDate { get; set; }
    
    [Required]
    public string AddedToLibrary { get; set; }
    
    [Required]
    public string LastOpened { get; set; }
    
    [Required]
    public string Cover { get; set; }
    
    public string Data { get; set; }
    
    public IList<AuthorInDto> Authors { get; set; }

    
    public bool IsValid => CurrentPage <= Pages;
}