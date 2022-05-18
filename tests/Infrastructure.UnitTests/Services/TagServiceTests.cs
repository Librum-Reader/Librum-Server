using System.Collections.Generic;
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
    private readonly IMapper _mapper;
    private readonly ITagService _tagService;
    
    
    public TagServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<TagAutoMapperProfile>());
        _mapper = new Mapper(mapperConfig);

        _tagService = new TagService(_mapper, _tagRepositoryMock.Object, _userRepositoryMock.Object);
    }


    [Fact]
    public async Task CreateTagAsync_ShouldCallSaveChanges_WhenUserExistsAndTagUnique()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Tags = new List<Tag>() });

        _userRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<User>()));
        
        _tagRepositoryMock.Setup(x => x.Exists(new User(), new TagInDto()))
            .Returns(false);

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

        _tagRepositoryMock.Setup(x => x.Exists(new User(), new TagInDto()))
            .Returns(false);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.CreateTagAsync("JohnDoe@gmail.com", new TagInDto()));
    }
    
    [Fact]
    public async Task CreateTagAsync_ShouldThrow_WhenTagNameAlreadyExists()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);
        
        _userRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<User>()));
        
        _tagRepositoryMock.Setup(x => x.Exists(new User(), new TagInDto()))
            .Returns(true);

        
        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.CreateTagAsync("JohnDoe@gmail.com", new TagInDto()));
    }
    
    [Fact]
    public async Task DeleteTagAsync_ShouldCallSaveChangesAsync_WhenUserExistsAndTagExists()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User { Tags = new List<Tag>() });
        
        _userRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<User>()));
        
        _tagRepositoryMock.Setup(x => x.Get(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(() => new Tag());
        
        _tagRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        await _tagService.DeleteTagAsync("JohnDoe@gmail.com", "MyTag");
        
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
            .ReturnsAsync(new User());

        _userRepositoryMock.Setup(x => x.LoadRelationShipsAsync(It.IsAny<User>()));
        
        _tagRepositoryMock.Setup(x => x.Get(It.IsAny<User>(), It.IsAny<string>()))
            .Returns(() => null);
        
        
        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _tagService.DeleteTagAsync("JohnDoe@gmail.com", "MyTag"));
    }
}