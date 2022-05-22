using System.ComponentModel.DataAnnotations;
using Application.Common.DTOs.Authors;
using Domain.Enums;

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
    
    public BookFormat Format { get; set; }
    
    public byte[] Data { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int CurrentPage { get; set; }
    
    public IList<AuthorInDto> Authors { get; set; }

    public bool IsValid => CurrentPage <= Pages && 
                           Authors.Count() == Authors.DistinctBy(x => new { x.FirstName, x.LastName }).Count();
}