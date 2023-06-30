using Application.Common.DTOs.Blogs;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Interfaces.Services;

public interface IBlogService
{
    Task<String> CreateBlogAsync(BlogInDto blogInDto);
    ICollection<BlogOutDto> GetAllBlogs();
    Task DeleteBlogAsync(Guid guid);
    
    Task AddBlogContent(Guid guid, MultipartReader reader);
    Task<Stream> GetBlogContent(Guid guid);
    
    Task<Stream> GetCover(Guid guid);
    Task ChangeCover(Guid guid, MultipartReader reader);
    Task DeleteCover(Guid guid);
}