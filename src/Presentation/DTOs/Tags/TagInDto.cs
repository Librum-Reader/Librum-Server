using System.ComponentModel.DataAnnotations;

namespace Presentation.DTOs.Tags;

public class TagInDto
{
    [Required]
    [MinLength(2, ErrorMessage = "The provided tag name is too short")]
    [MaxLength(30, ErrorMessage = "The provided tag name is too long")]
    public string Name { get; set; }
}