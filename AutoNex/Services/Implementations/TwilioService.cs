using AutoNex.Services.Interfaces;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AutoNex.Services.Implementations;

public class TwilioService : ITwilioService
{
    private readonly TwilioSettings _settings;

    public TwilioService(IOptions<TwilioSettings> settings)
    {
        _settings = settings.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrEmpty(_settings.AccountSid) &&
        !string.IsNullOrEmpty(_settings.AuthToken) &&
        !string.IsNullOrEmpty(_settings.FromNumber);

    public async Task<bool> SendWhatsAppAsync(string to, string message)
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
            );

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}
