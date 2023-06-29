using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Blog
{
    [Key]
    public Guid BlogId { get; set; }
    
    [Required]
    [MinLength(4, ErrorMessage = "The title is too short")]
    [MaxLength(500, ErrorMessage = "The title is too long")]
    public string Title { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "The introduction is too short")]
    [MaxLength(1000, ErrorMessage = "The introduction is too long")]
    public string Introduction { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }

    public bool HasCover { get; set; } = false;
}