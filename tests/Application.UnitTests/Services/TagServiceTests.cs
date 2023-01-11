using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Mappings;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services.v1;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly ITagService _tagService;
    
    
    public TagServiceTests()
    {
        var mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<TagAutoMapperProfile>());
        var mapper = new Mapper(mapperConfig);

        _tagService = new TagService(mapper, _tagRepositoryMock.Object,
                                     _userRepositoryMock.Object);
    }

    [Fact]
    public async Task ATagService_SucceedsDeletingATag()
    {
        // Arrange
        var tagGuid = Guid.NewGuid();
        
        var user = new User
        {
            Tags = new List<Tag>
            {
                new Tag
                {
                    TagId = tagGuid,
                    Name = "someName"
                },
                new Tag
                {
                    TagId = Guid.NewGuid(),
                    Name = "someOtherName"
                }
            }
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);
        
        _tagRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        await _tagService.DeleteTagAsync("JohnDoe@gmail.com", tagGuid.ToString());
        
        // Assert
        _tagRepositoryMock.Verify(x => x.Delete(It.IsAny<Tag>()), Times.Once);
        _tagRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ATagService_SucceedsGettingTags()
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
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(user);
        
        // Act
        var result = (await _tagService.GetTagsAsync("JohnDoe@gmail.com")).ToList();

        // Assert
        for(var i = 0; i < tagNames.Length; ++i)
        {
            Assert.Equal(tagNames[i], result.ElementAt(i).Name);
        }
    }
}