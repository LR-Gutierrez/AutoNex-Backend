using AutoNex.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AutoNex.Services.Implementations;

public class TwilioService : ITwilioService
{
    private readonly TwilioSettings _settings;
    private readonly ILogger<TwilioService> _logger;

    public TwilioService(IOptions<TwilioSettings> settings, ILogger<TwilioService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrEmpty(_settings.AccountSid) &&
        !string.IsNullOrEmpty(_settings.AuthToken) &&
        !string.IsNullOrEmpty(_settings.FromNumber);

    public async Task<bool> SendWhatsAppAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) return false;

        try
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var from = new PhoneNumber($"whatsapp:{_settings.FromNumber}");
            var toNumber = new PhoneNumber($"whatsapp:{to}");

            await MessageResource.CreateAsync(
                body: message,
                from: from,
                to: toNumber
            ).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje de WhatsApp a {To}", to);
            return false;
        }
    }
}

