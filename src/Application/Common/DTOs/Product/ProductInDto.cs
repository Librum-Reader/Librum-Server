using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Product;

public class ProductInDto
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public long BookStorageLimit { get; set; }
    
    [Required]
    public int AiRequestLimit { get; set; }
    
    [Required]
    public int TranslationsLimit { get; set; }
    
    [Required]
    public bool LiveMode { get; set; }

    [Required]
    public ICollection<string> Features { get; set; } = new Collection<string>();
}