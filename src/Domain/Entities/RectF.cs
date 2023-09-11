using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class RectF
{
    [Key]
    public Guid RectFId { get; set; }
    
    [Required]
    public float X { get; set; }
    
    [Required]
    public float Y { get; set; }
    
    [Required]
    public float Width { get; set; }
    
    [Required]
    public float Height { get; set; }
    
    public Guid HighlightId { get; set; }
    public Highlight Highlight { get; set; }
}