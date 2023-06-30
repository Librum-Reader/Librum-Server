using Application.Common.DTOs.Blogs;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;

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

        await _blogBlobStorageManager.DeleteBlogContent(guid.ToString());
        if (blog.HasCover)
            await DeleteCover(guid);

        _blogRepository.DeleteBlog(blog);
        await _blogRepository.SaveChangesAsync();
    }

    
    public async Task AddBlogContent(Guid guid, MultipartReader reader)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }

        await _blogBlobStorageManager.AddBlogContent(guid.ToString(), reader);
    }

    public async Task<Stream> GetBlogContent(Guid guid)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }

        return await _blogBlobStorageManager.DownloadBlogContent(guid.ToString());
    }
    
    
    public async Task<Stream> GetCover(Guid guid)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }

        return await _blogBlobStorageManager.DownloadBlogCover(guid.ToString());
    }

    public async Task ChangeCover(Guid guid, MultipartReader reader)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }

        await _blogBlobStorageManager.ChangeBlogCover(guid.ToString(), reader);

        blog.HasCover = true;
        await _blogRepository.SaveChangesAsync();
    }

    public async Task DeleteCover(Guid guid)
    {
        var blog = await _blogRepository.GetBlogAsync(guid);
        if (blog == null)
        {
            var message = "No blog with this guid exists";
            throw new CommonErrorException(404, message, 0);
        }

        await _blogBlobStorageManager.DeleteBlogCover(guid.ToString());
        
        blog.HasCover = true;
        await _blogRepository.SaveChangesAsync();
    }
}