using Application.Common.DTOs.Blogs;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Services;

public interface IBlogService
{
    Task CreateBlogAsync(BlogInDto blogInDto);
    ICollection<BlogOutDto> GetAllBlogs();
    Task DeleteBlogAsync(Guid guid);
    
    Task<Stream> GetCover(Guid guid);
    Task ChangeCover(Guid guid, MultipartReader reader);
    Task DeleteCover(Guid guid);
}