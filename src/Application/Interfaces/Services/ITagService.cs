using Application.Common.DTOs.Tags;

namespace Application.Interfaces.Services;

public interface ITagService
{
    public Task CreateTagAsync(string email, TagInDto tagIn);
    public Task DeleteTagAsync(string email, string guid);
    public Task<IEnumerable<TagOutDto>> GetTagsAsync(string email);
}