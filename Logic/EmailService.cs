using Db.Context;
using Db.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Logic;

public interface IEmailService
{
    public Task SendEmailAsync(string email, string subject, string message);
    public Task<string> CreateAndAddToDbVerificationCodeAsync(string email);
    public Task<bool> VerifyEmailAsync(string email, string verificationCode);
    public Task<bool> IsEmailVerifiedAsync(string email);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly EmailConfirmationContext _emailConfirmationContext;

    public EmailService(IConfiguration configuration, EmailConfirmationContext emailConfirmationContext)
    {
        _configuration = configuration;
        _emailConfirmationContext = emailConfirmationContext;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(_configuration["EmailSettings:OfficialName"], _configuration["EmailSettings:EmailAddress"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(TextFormat.Html)
        {
            Text = message
        };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], _configuration.GetValue<int>("EmailSettings:SmtpPort"), true);
            await client.AuthenticateAsync(_configuration["EmailSettings:EmailAddress"], _configuration["EmailSettings:EmailPassword"]);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }

    public async Task<string> CreateAndAddToDbVerificationCodeAsync(string email)
    {
        var configExpireTime = _configuration.GetValue<int>("RegistrationSettings:VerificationCodeLifecycleMinutes");
        var expirationTime = DateTime.UtcNow.AddMinutes(configExpireTime);
        var code = CreateVerificationCode();

        var confirmationEntity = new EmailConfirmationEntity
            {Email = email, ExpirationTime = expirationTime, VerificationCode = code, IsVerified = false};
        await _emailConfirmationContext.AddConfirmationEntiytyAsync(confirmationEntity);

        return code;
    }

    public async Task<bool> VerifyEmailAsync(string email, string verificationCode)
    {
        var confirmationEntity = await _emailConfirmationContext.FindConfirmationEntityAsync(email);
        if (confirmationEntity.VerificationCode == verificationCode)
        {
            if (confirmationEntity.ExpirationTime <= DateTime.UtcNow)
            {
                await _emailConfirmationContext.RemoveConfirmationEntityAsync(email);
                throw new TimeoutException("Email verification code expired");
            }
            
            confirmationEntity.IsVerified = true;
            confirmationEntity.ExpirationTime =
                DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("RegistrationSettings:VerificationCodeLifecycleMinutes"));
            await _emailConfirmationContext.UpdateConfirmationEntityAsync(confirmationEntity);
        }

        return confirmationEntity.VerificationCode == verificationCode;
    }

    public async Task<bool> IsEmailVerifiedAsync(string email)
    {
        var confirmationEntity = await _emailConfirmationContext.FindConfirmationEntityAsync(email);
        if (confirmationEntity.ExpirationTime < DateTime.UtcNow)
        {
            await _emailConfirmationContext.RemoveConfirmationEntityAsync(email);
            throw new TimeoutException("Email verification has expired");
        }
        return confirmationEntity.IsVerified;
    }

    private string CreateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}