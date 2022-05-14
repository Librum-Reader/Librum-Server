using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Common.DTOs.User;
using Domain.Entities;
using Infrastructure.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Infrastructure.UnitTests;

public class AuthenticationManagerTests
{
    private readonly AuthenticationManager _authenticationManager;
    private readonly Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();
    private readonly Mock<UserManager<User>> _userManagerMock = TestHelpers.MockUserManager<User>();


    public AuthenticationManagerTests()
    {
        _authenticationManager = new AuthenticationManager(_configurationMock.Object, _userManagerMock.Object);
    }


    [Fact]
    public async Task UserExistsAsync_ShouldReturnTrue_WhenEmailAndPasswordMatch()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        bool result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com", "MyPassword123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        bool result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com", "MyPassword123");

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        bool result = await _authenticationManager.UserExistsAsync("JohnDoe@gmail.com", "MyPassword123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateTokenAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        // Arrange
        _configurationMock.Setup(p => p[It.IsAny<string>()])
            .Returns("SomeString12345");
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _authenticationManager.CreateTokenAsync(null));
    }
    
    [Fact]
    public async Task CreateTokenAsync_ShouldReturnToken_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Email = "johnDoe@gmail.com",
            UserName = "johnDoe@gmail.com",
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        
        var loginDto = new LoginDto
        {
            Email = "JohnDoe@gmail.com",
            Password = "MyPassword123"
        };
        
        _configurationMock.Setup(x => x[It.IsAny<string>()])
            .Returns("SomeLongongongongongongongongongongongongString12345");

        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsNotNull<string>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>() {"Manager", "User"});
        
        
        // Act
        string result = await _authenticationManager.CreateTokenAsync(loginDto);

        // Assert
        Assert.NotEmpty(result);
    }
}