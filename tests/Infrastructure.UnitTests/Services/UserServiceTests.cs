using System;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Services.v1;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Infrastructure.UnitTests.Services;

public class UserServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
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
        var user = new User
        {
            Email = "johnDoe@gmail.com",
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(user.Email, false))
            .ReturnsAsync(user); 
        
        
        // Act
        var result = await _userService.GetUserAsync("johnDoe@gmail.com");
        
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
}