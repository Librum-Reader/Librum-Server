using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entities;


public class Book
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid BookId { get; set; }

    [Required]
    [MinLength(4, ErrorMessage = "The book title is too short")]
    [MaxLength(120, ErrorMessage = "The book title is too long")]
    public string Title { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The amount of pages is not in bounds")]
    public int Pages { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The current page is not in bounds")]
    public int CurrentPage { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "The format is too short")]
    [MaxLength(40, ErrorMessage = "The format is too long")]
    public string Format { get; set; }
    
    [MinLength(2, ErrorMessage = "The language is too short")]
    [MaxLength(50, ErrorMessage = "The language is too long")]
    public string Language { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The document size is too short")]
    [MaxLength(30, ErrorMessage = "The document size is too long")]
    public string DocumentSize { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "The pages size is too short")]
    [MaxLength(40, ErrorMessage = "The pages size is too long")]
    public string PagesSize { get; set; }
    
    [MinLength(2, ErrorMessage = "The creator is too short")]
    [MaxLength(140, ErrorMessage = "The creator is too long")]
    public string Creator { get; set; }
    
    [Required]
    public string DataLink { get; set; }
    
    [Required]
    public string CoverLink { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "The release date is too short")]
    [MaxLength(140, ErrorMessage = "The release date is too long")]
    public string ReleaseDate { get; set; }
    
    [Required]
    public string AddedToLibrary { get; set; }
    
    public string LastOpened { get; set; }
    
    
    public ICollection<Tag> Tags { get; set; }
    
    public ICollection<Author> Authors { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}
