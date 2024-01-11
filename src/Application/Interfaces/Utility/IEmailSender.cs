using Domain.Entities;

namespace Application.Interfaces.Utility;

public interface IEmailSender
{
    public Task SendEmailConfirmationEmail(User user, string token);
    public Task SendPasswordResetEmail(User user, string token);
    public Task SendDowngradeWarningEmail(User user);
}