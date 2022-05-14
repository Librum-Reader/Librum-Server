using Application.Common.DTOs;
using Application.Common.DTOs.User;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<UserOutDto> GetUserAsync(string email);
    public Task DeleteUserAsync(string email);
    public Task PatchUserAsync(string email, JsonPatchDocument<UserForUpdateDto> patchDoc, ControllerBase controllerBase);
}