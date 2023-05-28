using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Application.Services.v1;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;


    public UserService(IUserRepository userRepository, IBookRepository bookRepository,
                       IMapper mapper)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _mapper = mapper;
    }
    
    
    public async Task<UserOutDto> GetUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: false);
        var userOut = _mapper.Map<UserOutDto>(user);
        userOut.UsedBookStorage = await _bookRepository.GetUsedBookStorage(user.Id);

        return userOut;
    }

    public async Task DeleteUserAsync(string email)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task PatchUserAsync(string email,
                                     JsonPatchDocument<UserForUpdateDto> patchDoc,
                                     ControllerBase controllerBase)
    {
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        
        var userToPatch = _mapper.Map<UserForUpdateDto>(user);
        
        patchDoc.ApplyTo(userToPatch, controllerBase.ModelState);
        controllerBase.TryValidateModel(controllerBase.ModelState);

        if (!controllerBase.ModelState.IsValid || !userToPatch.DataIsValid)
        {
            const string message = "The provided data is invalid";
            throw new InvalidParameterException(message);
        }

        _mapper.Map(userToPatch, user);
        await _userRepository.SaveChangesAsync();
    }
}