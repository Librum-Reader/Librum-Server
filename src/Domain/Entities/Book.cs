using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;


namespace Domain.Entities;


public class Book
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid BookId { get; set; }

    [Required]
    [MinLength(4, ErrorMessage = "The provided book title is too short")]
    [MaxLength(120, ErrorMessage = "The provided book title is too long")]
    public string Title { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The provided amount of pages are not in bound")]
    public int Pages { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The provided amount of pages are not in bound")]
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
    public string DataLink { get; set; }
    
    [Required]
    public string CoverLink { get; set; }

    [Required]
    public string ReleaseDate { get; set; }
    
    [Required]
    public string AddedToLibrary { get; set; }
    
    public string LastOpened { get; set; }
    
    
    
    public ICollection<Tag> Tags { get; set; }
    
    public ICollection<Author> Authors { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}
