using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Services.v1;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new Mock<ITagRepository>();
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly ITagService _tagService;
    
    
    public TagServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TagAutoMapperProfile>());
        var mapper = new Mapper(mapperConfig);

        _tagService = new TagService(mapper, _tagRepositoryMock.Object, _userRepositoryMock.Object);
    }


    [Fact]
    public async Task CreateTagAsync_ShouldCallSaveChanges_WhenUserExistsAndTagUnique()
    {
        // Arrange
        var user = new User
        {
            Tags = new List<Tag>()
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        _tagRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        

        // Act
        await _tagService.CreateTagAsync("JohnDoe@gmial.com", new TagInDto());

        // Assert
        _tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateTagAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.CreateTagAsync("JohnDoe@gmail.com", new TagInDto()));
    }
    
    [Fact]
    public async Task CreateTagAsync_ShouldThrow_WhenTagNameAlreadyExists()
    {
        // Arrange
        const string tagName = "TagOne";
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag { Name = tagName }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => 
            _tagService.CreateTagAsync("JohnDoe@gmail.com", new TagInDto { Name = tagName}));
    }
    
    [Fact]
    public async Task DeleteTagAsync_ShouldCallSaveChangesAsync_WhenUserExistsAndTagExists()
    {
        // Arrange
        const string tagName = "MyTag";
        
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag { Name = tagName }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);
        
        _tagRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        await _tagService.DeleteTagAsync("JohnDoe@gmail.com", tagName);
        
        // Assert
        _tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteTagAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);

        
        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.DeleteTagAsync("JohnDoe@gmail.com", "MyTag"));
    }
    
    [Fact]
    public async Task DeleteTagAsync_ShouldThrow_WhenTagDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Tags = new List<Tag>() });
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.DeleteTagAsync("JohnDoe@gmail.com", "MyTag"));
    }
    
    [Fact]
    public async Task GetTagsAsync_ShouldReturnsAllTags_WhenDataIsValid()
    {
        // Arrange
        var tagNames = new string[] { "FirstTag", "SecondTag", "ThirdTag" };
        
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag { Name = tagNames[0] },
                new Tag { Name = tagNames[1] },
                new Tag { Name = tagNames[2] }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(user);
        
        // Act
        var result = await _tagService.GetTagsAsync("JohnDoe@gmail.com");

        // Assert
        for(int i = 0; i < tagNames.Length; ++i)
        {
            Assert.Equal(tagNames[i], result.ElementAt(i).Name);
        }
    }
    
    [Fact]
    public async Task GetTagsAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);
        
        
        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.GetTagsAsync("JohnDoe@gmail.com"));
    }
}