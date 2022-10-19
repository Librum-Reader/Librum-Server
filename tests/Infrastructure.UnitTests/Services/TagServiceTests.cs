using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.DTOs.Tags;
using Application.Common.Mappings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services.v1;
using AutoMapper;
using Domain.Entities;
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
    public async Task GetTagsAsync_ShouldReturnsAllTags_WhenDataIsValid()
    {
        // Arrange
        var tagNames = new[] { "FirstTag", "SecondTag", "ThirdTag" };
        
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
}