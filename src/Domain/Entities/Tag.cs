using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Tag
{
    [Key]
    public Guid TagId { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The provided tag name is too short")]
    [MaxLength(30, ErrorMessage = "The provided tag name is too long")]
    public string Name { get; set; }
    
    
    
    public ICollection<Book> Books { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}