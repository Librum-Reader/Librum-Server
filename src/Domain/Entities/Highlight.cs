using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Highlight
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid HighlightId { get; set; }
    
    [Required]
    [MaxLength(500, ErrorMessage = "The color is too long")]
    public string Color { get; set; }
    
    [Required]
    public int PageNumber { get; set; }
    
    [Required]
    public ICollection<RectF> Rects { get; set; } = new List<RectF>();
    
    public Guid BookId { get; set; }
    public Book Book { get; set; }
}