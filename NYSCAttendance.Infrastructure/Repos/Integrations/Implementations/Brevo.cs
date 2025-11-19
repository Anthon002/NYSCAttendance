using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Integrations.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Repos.Integrations.Implementations;

public record class BrevoService : IBrevo
{
    private readonly IHttpClientFactory _client;
    private readonly AppSettingsOptions _options;
    private readonly ILogger<BrevoService> _logger;
    public BrevoService(IHttpClientFactory client, IOptionsSnapshot<AppSettingsOptions> options, ILogger<BrevoService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }
    public async Task<BaseResponse> SendEmail(BrevoRequest request, CancellationToken cancellationToken)
    {
        using (var client = _client.CreateClient())
        {
            try
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("api-key", _options.Brevo?.Secret);
                var url = _options.Brevo?.Baseurl + "/smtp/email";

                var payload = new StringContent(JsonSerializer.Serialize(new
                {
                    sender = new
                    {
                        name = _options.AppSettings?.AppName,
                        email = _options.AppSettings?.SupportEmail
                    },
                    to = new[]
                    {
                    new
                    {
                        email = request.RecipientEmail,
                        name = request.RecipientName
                    }
                },
                    subject = request.Subject,
                    htmlContent = request.HTMLContent
                }), Encoding.UTF8, MediaTypeNames.Application.Json);

                var response = await client.PostAsync(url, payload, cancellationToken);
                var httpsResponse = response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode)
                    return new BaseResponse(true, "Message sent successfully.");
                else
                {
                    return new BaseResponse(false, $"Message could not be sent. {httpsResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Brevo => Application ran into an error while trying to send email.");
                return new BaseResponse(false, "Application ran into an error.");
            }
        }
    }
}
