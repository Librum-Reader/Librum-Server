using System.ComponentModel.DataAnnotations;
namespace Application.Common.DTOs.Highlights;

public class RectFInDto
{
    [Required]
    public float X { get; set; }
    
    [Required]
    public float Y { get; set; }
    
    [Required]
    public float Width { get; set; }
    
    [Required]
    public float Height { get; set; }
}