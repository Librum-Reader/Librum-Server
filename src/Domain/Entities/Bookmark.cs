using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Bookmark
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid BookmarkId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public int PageNumber { get; set; }
    
    [Required]
    public float YOffset { get; set; }
    
    public Guid BookId { get; set; }
    public Book Book { get; set; }
}