using Microsoft.AspNetCore.Identity.UI.Services;

namespace NguyenHaiDang_W345.Services;

public class LocalEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
