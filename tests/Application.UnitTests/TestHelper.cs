using Microsoft.AspNetCore.Identity;
using Moq;

namespace Application.UnitTests;

public static class TestHelpers
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() 
        where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null,
                                               null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }
}