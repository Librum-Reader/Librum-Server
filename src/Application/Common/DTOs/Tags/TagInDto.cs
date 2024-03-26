using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Tags;

public class TagInDto
{
    [Required]
    public Guid Guid { get; set; }
    
    [Required]
    [MinLength(2, ErrorMessage = "The tag name is too short")]
    [MaxLength(5000, ErrorMessage = "The tag name is too long")]
    public string Name { get; set; }
}