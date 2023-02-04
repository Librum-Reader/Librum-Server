using Application.Common.DTOs.Tags;

namespace Application.Interfaces.Services;

public interface ITagService
{
    public Task DeleteTagAsync(string email, Guid guid);
    public Task UpdateTagAsync(string email, TagForUpdateDto tagDto);
    public Task<IEnumerable<TagOutDto>> GetTagsAsync(string email);
}