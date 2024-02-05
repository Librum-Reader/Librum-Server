using System.Collections.ObjectModel;

namespace Application.Common.DTOs.Folders;

public class FolderOutDto
{
    public string Guid { get; set; }
    
    public string Name { get; set; }
    
    public string Color { get; set; }
    
    public string Icon { get; set; }
    
    public string Description { get; set; }
    
    public string LastModified { get; set; }
    
    public ICollection<FolderOutDto> Children { get; set; } = new Collection<FolderOutDto>();
}