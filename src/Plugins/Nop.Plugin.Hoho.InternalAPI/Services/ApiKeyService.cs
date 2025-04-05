using System.Security.Cryptography;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;
using Nop.Services.Logging;
using SymetricEncryptor;

namespace Nop.Plugin.Hoho.ExternalAPIs.Services;

public interface IApiKeyService
{
    Task InsertApiKeyAsync(ApiKey apiKey);
    Task UpdateApiKeyAsync(ApiKey apiKey);
    Task DeleteApiKeyAsync(ApiKey apiKey);
    Task<ApiKey> GetApiKeyByIdAsync(int id);
    Task<ApiKey> GetApiKeyByEncryptedKeyAsync(string encryptedApiKey);
    Task<IList<ApiKey>> GetAllApiKeysAsync();

    Task AddApiRequestLogAsync(ApiRequestLog requestLog);
    
    string GetEncryptedAPIKey(string apiKey);
    string GetDecryptedAPIKey(string encryptedApiKey);
    string GenerateApiKey();
}

public class ApiKeyService : IApiKeyService
{
    private const string EN_KEY = "fa2ad2fa4f4fefe1";

    private readonly IRepository<ApiKey> _apiKeyRepository;
    private readonly IRepository<ApiRequestLog> _apiRequestLogRepository;
    private readonly ILogger _logger;
    private readonly IStaticCacheManager _staticCacheManager;

    public ApiKeyService(IRepository<ApiKey> apiKeyRepository,
        IRepository<ApiRequestLog> apiRequestLogRepository,
        ILogger logger,
        IStaticCacheManager staticCacheManager)
    {
        _apiKeyRepository = apiKeyRepository;
        _apiRequestLogRepository = apiRequestLogRepository;
        _logger = logger;
        _staticCacheManager = staticCacheManager;
    }



    public async Task InsertApiKeyAsync(ApiKey apiKey)
    {
        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        await _apiKeyRepository.InsertAsync(apiKey);
    }

    public async Task UpdateApiKeyAsync(ApiKey apiKey)
    {
        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        await _apiKeyRepository.UpdateAsync(apiKey);
    }

    public async Task DeleteApiKeyAsync(ApiKey apiKey)
    {
        if (apiKey == null)
            throw new ArgumentNullException(nameof(apiKey));

        await _apiKeyRepository.DeleteAsync(apiKey);
        await _staticCacheManager.RemoveAsync(NopEntityCacheDefaults<ApiKey>.ByIdCacheKey, apiKey.Id);
    }

    public async Task<ApiKey> GetApiKeyByIdAsync(int id)
    {
        return await _apiKeyRepository.GetByIdAsync(id, cache => default, useShortTermCache: true);
    }

    public async Task<ApiKey> GetApiKeyByEncryptedKeyAsync(string encryptedApiKey)
    {
        var allrecords = await GetAllApiKeysAsync();
        return allrecords.FirstOrDefault(x => x.EncryptedKey == encryptedApiKey);
    }

    public async Task<IList<ApiKey>> GetAllApiKeysAsync()
    {
        return await _apiKeyRepository.GetAllAsync(query => query);
    }

    public async Task AddApiRequestLogAsync(ApiRequestLog requestLog)
    {
        if (requestLog == null)
            throw new ArgumentNullException(nameof(requestLog));

        await _apiRequestLogRepository.InsertAsync(requestLog);
        _logger.Information($"API request logged for API Key ID {requestLog.ApiKeyId}");
    }


    public string GetEncryptedAPIKey(string apiKey)
    {
        return EncryptionDecryptionManager.Encrypt(apiKey, EN_KEY);
    }

    public string GetDecryptedAPIKey(string encryptedApiKey)
    {
        return EncryptionDecryptionManager.Decrypt(encryptedApiKey, EN_KEY);
    }

    public string GenerateApiKey()
    {
        byte[] key = new byte[32];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(key);
        }

        return Convert.ToBase64String(key);
    }
}
