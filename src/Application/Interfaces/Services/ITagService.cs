using Application.Common.DTOs.Tags;

namespace Application.Interfaces.Services;

public interface ITagService
{
    public Task DeleteTagAsync(string email, string guid);
    public Task UpdateTagAsync(string email, string guid,
                               TagForUpdateDto tagUpdate);
    public Task<IEnumerable<TagOutDto>> GetTagsAsync(string email);
}