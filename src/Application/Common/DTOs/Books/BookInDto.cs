using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Authors;

namespace Application.Common.DTOs.Books;

public class BookInDto
{
    [Required]
    [MinLength(4, ErrorMessage = "The provided title is too short")]
    [MaxLength(80, ErrorMessage = "The provided title is too long")]
    public string Title { get; set; }
    
    public DateTime ReleaseDate { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Pages { get; set; }
    
    [MaxLength(40, ErrorMessage = "The provided book format name is too long")]
    public string Format { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }
    
    public IList<AuthorInDto> Authors { get; set; }
}