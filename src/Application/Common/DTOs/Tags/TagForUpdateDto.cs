using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Tags;

public class TagForUpdateDto
{
    [Required]
    public Guid Guid { get; set; }
    
    [MinLength(2, ErrorMessage = "The tag name is too short")]
    [MaxLength(30, ErrorMessage = "The tag name is too long")]
    public string Name { get; set; }
}