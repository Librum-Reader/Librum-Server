using Application.Common.DTOs.Tags;

namespace Application.Common.Interfaces.Services;

public interface ITagService
{
    public Task CreateTagAsync(string email, TagInDto tagIn);
    public Task DeleteTagAsync(string email, string tagName);
}