using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(Title), IsUnique = true)]
public class Book
{
    [Key]
    public Guid BookId { get; set; }

    [Required]
    [MinLength(4, ErrorMessage = "The provided book title is too short")]
    [MaxLength(120, ErrorMessage = "The provided book title is too long")]
    public string Title { get; set; }

    public DateTime ReleaseDate { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The provided amount of pages are not in bound")]
    public int Pages { get; set; }

    [Required]
    public BookFormat Format { get; set; }
    
    [Required]
    public string DataLink { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The provided amount of pages are not in bound")]
    public int CurrentPage { get; set; }
    
    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    public DateTime LastOpened { get; set; }
    
    
    
    public ICollection<Tag> Tags { get; set; }
    
    public ICollection<Author> Authors { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}
