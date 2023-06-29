using Application.Common.DTOs.Blogs;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class BlogService : IBlogService
{
    private readonly IMapper _mapper;
    private readonly IBlogRepository _blogRepository;
    private readonly IBlogBlobStorageManager _blogBlobStorageManager;

    public BlogService(IMapper mapper, IBlogRepository blogRepository,
                       IBlogBlobStorageManager blogBlobStorageManager)
    {
        _mapper = mapper;
        _blogRepository = blogRepository;
        _blogBlobStorageManager = blogBlobStorageManager;
    }

    public async Task CreateBlogAsync(BlogInDto blogInDto)
    {
        var blog = _mapper.Map<Blog>(blogInDto);
        await _blogRepository.AddBlogAsync(blog);

        await _blogRepository.SaveChangesAsync();
    }

    public ICollection<BlogOutDto> GetAllBlogs()
    {
        var blogs = _blogRepository.GetAll().ToList();
        
        return blogs.Select(blog => _mapper.Map<BlogOutDto>(blog)).ToList();
    }

    public async Task DeleteBlogAsync(Guid guid)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }
        
        _blogRepository.DeleteBlog(blog);
        await _blogRepository.SaveChangesAsync();
    }
}