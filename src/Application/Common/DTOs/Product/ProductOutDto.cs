using System.Collections.ObjectModel;

namespace Application.Common.DTOs.Product;

public class ProductOutDto
{
    public string Id { get; set; }
    
    public bool Active { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public int Price { get; set; }
    
    public string PriceId { get; set; }
    
    public bool LiveMode { get; set; }

    public ICollection<string> Features { get; set; } = new Collection<string>();
}