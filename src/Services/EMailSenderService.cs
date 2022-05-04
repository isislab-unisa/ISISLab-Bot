namespace ISISLab.HelpDesk.Services
{
  using System;
  using System.Net;
  using System.Net.Mail;

  using ISISLab.HelpDesk.Logging;
  using Microsoft.Extensions.Options;

  public class EmailSenderService
  {
    private readonly ILogger _logger;
    private readonly AppOptions _appOptions;

    public EmailSenderService(ILogger<EmailSenderService> logger, IOptions<AppOptions> options)
    {
      _logger     = logger;
      _appOptions = options.Value;

      var configured = !string.IsNullOrEmpty(_appOptions.Google.Gmail.From) &&
                       !string.IsNullOrEmpty(_appOptions.Google.Gmail.Password);

      if (!configured)
        throw new Exception("Unable to initialize Email Service. Invalid configuration.");
    }

    public bool SendEmail(string subject, string body)
    {
      try
      {
        var fromAddress = new MailAddress(_appOptions.Google.Gmail.From, "ISISLab HelpDesk");
        var toAddress   = new MailAddress(_appOptions.Google.Gmail.To);

        var smtp = new SmtpClient
        {
          Host            = "smtp.gmail.com",
          Port            = 587,
          EnableSsl       = true,
          DeliveryMethod  = SmtpDeliveryMethod.Network,
          Credentials     = new NetworkCredential(fromAddress.Address, _appOptions.Google.Gmail.Password)
        };

        using (var message = new MailMessage(fromAddress, toAddress))
        {
          message.Subject = subject;
          message.Body    = body;

          smtp.Send(message);
        }

        return true;
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Error with email sender");
        return false;
      }
    }
  }
}
