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
    public string Name { get; set; }
    
    [Required]
    public string Color { get; set; }
    
    [Required]
    public string Icon { get; set; }
    
    public string Description { get; set; }
    
    [Required]
    public string LastModified { get; set; }
    
    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public List<Folder>? Children { get; set; } = new List<Folder>();
}