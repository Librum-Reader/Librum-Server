using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    [Key]
    public Guid TagId { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The provided tag name is too short")]
    [MaxLength(30, ErrorMessage = "The provided tag name is too long")]
    public string Name { get; set; }

    public DateTime CreationDate { get; set; }
    
    
    
    public ICollection<Book> Books { get; set; }

    public string UserId { get; set; }
    public User User { get; set; }
}