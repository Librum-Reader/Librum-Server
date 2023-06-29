using Application.Common.DTOs.Blogs;

namespace Application.Interfaces.Services;

public interface IBlogService
{
    Task CreateBlogAsync(BlogInDto blogInDto);
    ICollection<BlogOutDto> GetAllBlogs();
    Task DeleteBlogAsync(Guid guid);
}