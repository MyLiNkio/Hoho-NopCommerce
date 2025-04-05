using Microsoft.AspNetCore.Http;
using Nop.Plugin.Hoho.ExternalAPIs.Services;
using Nop.Services.Logging;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;
using Nop.Services.Security;

namespace Nop.Plugin.Hoho.ExternalAPIs.Middleware
{
    public class ApiKeyValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY_HEADER_NAME = "X-Api-Key";
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger _logger;

        public ApiKeyValidationMiddleware(RequestDelegate next, IApiKeyService apiKeyService, ILogger logger)
        {
            _next = next;
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Does path contains "HohoAPI"?
            if (!context.Request.Path.Value.ToLower().Contains(($"/{HohoExternalAPIsPluginSettings.ApiPath}").ToLower()))
            {
                // If not — move to other middleware
                await _next(context);
                return;
            }

            // Extract API key from header
            if (!context.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key was not provided.");
                await _logger.WarningAsync("Suspicious activity detected. Someone tried to call the API without the appropriate key.");
                return;
            }

            //Encript API key from header to compare it with and available in DB
            var encryptedApiKey = _apiKeyService.GetEncryptedAPIKey(extractedApiKey);

            // Get API key from DB
            var apiKeyEntity = await _apiKeyService.GetApiKeyByEncryptedKeyAsync(encryptedApiKey);

            //Check if API key exist
            if (apiKeyEntity == null)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("Invalid API Key.");
                await _logger.WarningAsync("Suspicious activity detected. Someone tried to call the API with incorrect API key.");
                return;
            }

            //Check if API key is available
            if (apiKeyEntity == null || !apiKeyEntity.IsEnabled)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("API Key is not available.");
                await LogApiKeyUsage(context, apiKeyEntity, false);
                await _logger.WarningAsync("Suspicious activity detected. Someone tried to call the API with API key which is disabled.");
                return;
            }

            //Check if API key not expired
            if (apiKeyEntity.ExpirationDateUtc.HasValue && apiKeyEntity.ExpirationDateUtc.Value < DateTime.UtcNow)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("API Key has expired.");
                await LogApiKeyUsage(context, apiKeyEntity, false);
                await _logger.WarningAsync("Suspicious activity detected. Someone tried to call the API with API key which is expired.");
                return;
            }

            await LogApiKeyUsage(context, apiKeyEntity, true);

            await _next(context);
        }

        private async Task LogApiKeyUsage(HttpContext context, ApiKey apiKeyEntity, bool isSucces)
        {
            var apiRequestLog = new ApiRequestLog
            {
                ApiKeyId = apiKeyEntity.Id,
                IsSuccessUsage = isSucces,
                RequestUrl = $"{context.Request.Method} {context.Request.Path}",
                RequestMethod = context.Request.Method,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                RequestTimeUtc = DateTime.UtcNow,
                UserId = apiKeyEntity.UserId
            };

            await _apiKeyService.AddApiRequestLogAsync(apiRequestLog);
        }
    }

}
