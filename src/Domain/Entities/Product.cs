using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public string ProductId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public int Price { get; set; }
    
    [Required]
    public long BookStorageLimit { get; set; }
    
    [Required]
    public int AiRequestLimit { get; set; }

    public ICollection<ProductFeature> Features { get; set; } = new List<ProductFeature>();
}