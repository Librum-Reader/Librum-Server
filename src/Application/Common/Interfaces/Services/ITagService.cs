using Application.Common.DTOs.Tags;

namespace Application.Common.Interfaces.Services;

public interface ITagService
{
    public Task CreateTagAsync(string userEmail, TagInDto tagIn);
}