using System.Collections.ObjectModel;

namespace Application.Common.DTOs.Product;

public class ProductForUpdateDto
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public int Price { get; set; }
    
    public long BookStorageLimit { get; set; }
    
    public int AiRequestLimit { get; set; }

    public ICollection<string> Features { get; set; } = new Collection<string>();
}