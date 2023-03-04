using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Tag
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid TagId { get; set; }
    
    [MinLength(2, ErrorMessage = "The tag name is too short")]
    [MaxLength(30, ErrorMessage = "The tag name is too long")]
    public string Name { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }
    
    
    public string UserId { get; set; }
    public User User { get; set; }
}