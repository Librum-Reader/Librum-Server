using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Author
{
    [Key]
    public Guid AuthorId { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The provided firstname is too short")]
    [MaxLength(40, ErrorMessage = "The provided firstname is too long")]
    public string FirstName { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "The provided lastname is too short")]
    [MaxLength(50, ErrorMessage = "The provided lastname is too long")]
    public string LastName { get; set; }

    
    
    public Guid BookId { get; set; }
    public Book Book { get; set; }
}