using System;
using System.Threading.Tasks;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Utility;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IUserBlobStorageManager> _userBlobStorageManagerMock = new();
    private readonly Mock<IBookBlobStorageManager> _bookBlobStorageManagerMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = 
        new(new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
    private readonly Mock<ControllerBase> _controllerBaseMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly UserService _userService;
    
    
    public UserServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserAutoMapperProfile>();
        });
        
        _mapper = new Mapper(mapperConfig);
        _userService = new UserService(_userRepositoryMock.Object, 
                                       _bookRepositoryMock.Object,
                                       _userBlobStorageManagerMock.Object,
                                       _bookBlobStorageManagerMock.Object,
                                       _mapper, _emailSenderMock.Object, 
                                       _configurationMock.Object,
                                       _userManagerMock.Object,
                                       _productRepositoryMock.Object
            );
    }

    [Fact]
    public async Task AUserService_SucceedsDeletingAUser()
    {
        // Arrange
        const string userEmail = "johnDoe@gmail.com";

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), 
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User());

        
        // Act
        await _userService.DeleteUserAsync(userEmail);

        // Assert
        _userRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AUserService_SucceedsPatchingAUser()
    {
        // Arrange
        var patchDoc = new JsonPatchDocument<UserForUpdateDto>();
        patchDoc.Add(x => x.FirstName, "John");
        patchDoc.Add(x => x.LastName, "Doe");
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User());
        
        _controllerBaseMock.Setup(x => x.TryValidateModel(
                                      It.IsAny<ModelStateDictionary>()))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        
        // Act
        await _userService.PatchUserAsync("JohnDoe@gmail.com", patchDoc,
                                          _controllerBaseMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AUserService_FailsPatchingAUserIfDataIsIncorrect()
    {
        // Arrange
        var localControllerBaseMock = new Mock<ControllerBase>(); 
        localControllerBaseMock.Object.ModelState.AddModelError("key", "fail");
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(),
                                                  It.IsAny<bool>()))
            .ReturnsAsync(new User());

        localControllerBaseMock.Setup(x => x.TryValidateModel(
                                          It.IsAny<ModelStateDictionary>()))
            .Returns(true);
        
        
        // Assert
        await Assert.ThrowsAsync<CommonErrorException>(
            () => _userService.PatchUserAsync("JohnDoe@gmail.com", 
                                              new JsonPatchDocument<UserForUpdateDto>(),
                                              localControllerBaseMock.Object));
    }
}