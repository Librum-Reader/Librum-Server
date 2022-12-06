using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Tag
{
    [Key]
    [MinLength(2, ErrorMessage = "The tag name is too short")]
    [MaxLength(30, ErrorMessage = "The tag name is too long")]
    public string Name { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }

    
    public ICollection<Book> Books { get; set; }
    
    public string UserId { get; set; }
    public User User { get; set; }
}