#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Folder
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid FolderId { get; set; }
    
    [Required]
    [MaxLength(5000, ErrorMessage = "The name is too long")]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(500, ErrorMessage = "The color is too long")]
    public string Color { get; set; }
    
    [Required]
    [MaxLength(500, ErrorMessage = "The icon name is too long")]
    public string Icon { get; set; }
    
    [MaxLength(10000, ErrorMessage = "The description is too long")]
    public string Description { get; set; }
    
    [Required]
    [MaxLength(500, ErrorMessage = "The last modified is too long")]
    public string LastModified { get; set; }

    public int IndexInParent { get; set; }
    
    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public List<Folder>? Children { get; set; } = new List<Folder>();
}