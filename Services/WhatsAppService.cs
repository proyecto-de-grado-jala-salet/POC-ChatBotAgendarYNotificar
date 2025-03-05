using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Services.Interfaces;

namespace Services;

/// <summary>
/// This class sends text messages using WhatsApp.
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string? _token;
    private readonly string? _idCelphone;

    /// <summary>
    /// This constructor sets up the WhatsApp service with an HTTP client and configuration.
    /// </summary>
    /// <param name="httpClient">The HTTP client to send requests.</param>
    /// <param name="configuration">The configuration that holds WhatsApp credentials.</param>
    public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _token = configuration["WhatsApp:Token"];
        _idCelphone = configuration["WhatsApp:IdCelphone"];
    }

    /// <summary>
    /// Sends a text message to a given phone number via WhatsApp.
    /// </summary>
    /// <param name="phone">The recipient's phone number.</param>
    /// <param name="message">The text message to send.</param>
    public async Task SendTextMessageAsync(string phone, string message)
    {
        var url = $"https://graph.facebook.com/v21.0/{_idCelphone}/messages";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = phone,
            type = "text",
            text = new { body = message }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}