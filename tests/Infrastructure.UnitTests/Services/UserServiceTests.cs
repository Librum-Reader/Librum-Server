using System;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Services.v1;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class UserServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly Mock<ControllerBase> _controllerBaseMock = new Mock<ControllerBase>();
    private readonly UserService _userService;
    
    public UserServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile<UserAutoMapperProfile>(); });
        _mapper = new Mapper(mapperConfig);
        _userService = new UserService(_userRepositoryMock.Object, _mapper);
    }
    
    
    [Fact]
    public async Task GetUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        const string userEmail = "johnDoe@gmail.com";
        
        var user = new User
        {
            Email = userEmail,
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(user.Email, false))
            .ReturnsAsync(user); 
        
        
        // Act
        var result = await _userService.GetUserAsync(userEmail);
        
        // Assert
        Assert.Equal(JsonConvert.SerializeObject(_mapper.Map<UserOutDto>(user)), JsonConvert.SerializeObject(result));
    }
    
    [Fact]
    public async Task GetUserAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), false))
            .ReturnsAsync(() => null);


        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _userService.GetUserAsync("johnDoe@gmail.com"));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
    {
        // Arrange
        const string userEmail = "johnDoe@gmail.com";
        
        var user = new User
        {
            Email = userEmail,
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        
        // Act
        await _userService.DeleteUserAsync(userEmail);

        // Assert
        _userRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteUserAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        const string userEmail = "johnDoe@gmail.com";
        
        var user = new User
        {
            Email = userEmail,
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(() => _userService.DeleteUserAsync(userEmail));
    }

    [Fact]
    public async Task PatchUserAsync_ShouldCallSaveChangesAsync_WhenUserExistsAndDataIsValid()
    {
        // Arrange
        var patchDoc = new JsonPatchDocument<UserForUpdateDto>();
        patchDoc.Add(x => x.FirstName, "John");
        patchDoc.Add(x => x.LastName, "Doe");
        patchDoc.Add(x => x.Email, "JohnDoe@gmail.com");
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());
        
        _controllerBaseMock.Setup(x => x.TryValidateModel(It.IsAny<ModelStateDictionary>()))
            .Returns(true);
        
        _userRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        await _userService.PatchUserAsync("JohnDoe@gmail.com", patchDoc, _controllerBaseMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task PatchUserAsync_ShouldThrow_WhenTheUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(() => null);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _userService.PatchUserAsync("JohnDoe@gmail.com", new JsonPatchDocument<UserForUpdateDto>(), _controllerBaseMock.Object));
    }
    
    [Fact]
    public async Task PatchUserAsync_ShouldThrow_WhenThePatchDataIsWrong()
    {
        // Arrange
        var localControllerBaseMock = new Mock<ControllerBase>(); 
        localControllerBaseMock.Object.ModelState.AddModelError("Making it fail", "Because it needs to");
        
        _userRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new User());

        localControllerBaseMock.Setup(x => x.TryValidateModel(It.IsAny<ModelStateDictionary>()))
            .Returns(true);
        

        // Assert
        await Assert.ThrowsAsync<InvalidParameterException>(
            () => _userService.PatchUserAsync("JohnDoe@gmail.com", new JsonPatchDocument<UserForUpdateDto>(), localControllerBaseMock.Object));
    }
}