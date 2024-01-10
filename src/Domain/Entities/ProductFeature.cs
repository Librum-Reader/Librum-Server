using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class ProductFeature
{
    [Key]
    public Guid ProductFeatureId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string ProductId { get; set; }
    public Product Product { get; set; }
}