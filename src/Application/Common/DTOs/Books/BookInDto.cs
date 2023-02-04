using System.ComponentModel.DataAnnotations;
using Application.Common.DataAnnotations;
using Application.Common.DTOs.Tags;


namespace Application.Common.DTOs.Books;


public class BookInDto
{
    [Required]
    public Guid Guid { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The title is too short")]
    [MaxLength(200, ErrorMessage = "The title is too long")]
    public string Title { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int PageCount { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "The format is too short")]
    [MaxLength(60, ErrorMessage = "The format is too long")]
    public string Format { get; set; }

    [EmptyOrMinLength(2, ErrorMessage = "The language is too short")]
    [MaxLength(60, ErrorMessage = "The language is too long")]
    public string Language { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The document size is too short")]
    [MaxLength(60, ErrorMessage = "The document size is too long")]
    public string DocumentSize { get; set; }
    
    [MinLength(2, ErrorMessage = "The pages size is too short")]
    [MaxLength(60, ErrorMessage = "The pages size is too long")]
    public string PagesSize { get; set; }
    
    [EmptyOrMinLength(2, ErrorMessage = "The creator is too short")]
    [MaxLength(200, ErrorMessage = "The creator is too long")]
    public string Creator { get; set; }
    
    [EmptyOrMinLength(2, ErrorMessage = "The authors are too short")]
    [MaxLength(400, ErrorMessage = "The authors are too long")]
    public string Authors { get; set; }
    
    [EmptyOrMinLength(4, ErrorMessage = "The creation date is too short")]
    [MaxLength(100, ErrorMessage = "The creation date is too long")]
    public string CreationDate { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The added to library date is too short")]
    [MaxLength(100, ErrorMessage = "The added to library date is too long")]
    public string AddedToLibrary { get; set; }
    
    [EmptyOrMinLength(4, ErrorMessage = "The last opened is too short")]
    [MaxLength(100, ErrorMessage = "The last opened is too long")]
    public string LastOpened { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The last modified is too short")]
    [MaxLength(100, ErrorMessage = "The last modified is too long")]
    public string LastModified { get; set; }
    
    public string Cover { get; set; }
    
    public string Data { get; set; }

    public ICollection<TagInDto> Tags { get; set; } = new List<TagInDto>();


    public bool IsValid => CurrentPage <= PageCount;
}