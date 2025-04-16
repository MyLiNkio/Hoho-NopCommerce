using Newtonsoft.Json;
using System.Text;
using Nop.Web.Customization.Models.CustomNotification;

namespace Nop.Services.Customization;

public partial interface IMakeWebhookService
{
    Task SendWebhookAsync(CustomNotificationBase data);
}

public partial class MakeWebhookService: IMakeWebhookService
{
    private static readonly string MakeAPIKey = "Vnf7ZGglBT0j3KO5xYaMOkWAed5Hj4zXoP4IW4goxmw1HeFANl6xUXx6384uWvKySJFyTFmWKMGLCs2xWKf3748dnmyTN8TQfGGZ2t9A9cLxQ5CVU8wqzTobpYYBDpTX";
    private static readonly string MakeHookUrl = "https://hook.eu2.make.com/6te8ye21ep2d6zi8jt6shlz2rvtxm0uk";

    protected readonly Nop.Services.Logging.ILogger _logger;

    public MakeWebhookService(Nop.Services.Logging.ILogger logger)
    {
        _logger = logger;
    }

    public async Task SendWebhookAsync(CustomNotificationBase data)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, MakeHookUrl);
        request.Headers.Add("X-Api-Key", MakeAPIKey);

        request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await client.SendAsync(request);

        // Check if the response was not successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // Log the status code and the response content\
            await _logger.InformationAsync(message: $"Error: Request to {MakeHookUrl} failed with status code {(int)response.StatusCode} ({response.StatusCode}). Response content: {errorContent}");
        }
    }
}
