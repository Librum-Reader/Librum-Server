using System;
using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.Repository;
using Infrastructure.Services.v1;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Infrastructure.UnitTests;

public class UserServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserRepository> _userRepositoryMock = new Mock<IUserRepository>();
    private readonly Mock<ILogger<UserService>> _loggerMock = new Mock<ILogger<UserService>>();
    private readonly UserService _userService;
    
    public UserServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserAutoMapperProfile>();
        });
        _mapper = new Mapper(config);
        
        _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object, _mapper);
    }
    
    
    [Fact]
    public async void GetUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        User user = new User
        {
            Email = "johnDoe@gmail.com",
            AccountCreation = DateTime.Now,
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userRepositoryMock.Setup(x => x.GetAsync(user.Email))
            .ReturnsAsync(user);
        
        // Act
        var result = await _userService.GetUserAsync("myTestEmail@gmail.com");
        
        // Assert
        Assert.Equal(JsonConvert.SerializeObject(result), JsonConvert.SerializeObject(_mapper.Map<UserOutDto>(user)));
    }
}