using System.ComponentModel.DataAnnotations;
namespace Application.Common.DTOs.Highlights;

public class HighlightInDto
{
    [Required]
    public Guid Guid { get; set; }
    
    [Required]
    public string Color { get; set; }
    
    [Required]
    public int PageNumber { get; set; }
    
    [Required]
    public ICollection<RectFInDto> Rects { get; set; } = new List<RectFInDto>();
}