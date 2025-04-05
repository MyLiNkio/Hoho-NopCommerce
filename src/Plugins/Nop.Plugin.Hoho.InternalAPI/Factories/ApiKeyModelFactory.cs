using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;
using Nop.Plugin.Hoho.ExternalAPIs.Model;
using Nop.Plugin.Hoho.ExternalAPIs.Services;

namespace Nop.Plugin.Hoho.ExternalAPIs.Factories
{
    public interface IApiKeyModelFactory
    {
        Task<List<ApiKeyModel>> GetApiKeyModelList();

        Task CreateApiKey(ApiKeyModel model);

        Task<ApiKeyModel> GetApiKeyModel(int id);
        Task UpdateApiKey(ApiKeyModel model);

        Task DeleteApiKey(int id);
    }

    public class ApiKeyModelFactory : IApiKeyModelFactory
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyModelFactory(
            IApiKeyService apiKeyService
            )
        {
            _apiKeyService = apiKeyService;
        }

        public async Task<List<ApiKeyModel>> GetApiKeyModelList()
        {
            var apiKeysFromDB = await _apiKeyService.GetAllApiKeysAsync();

            var apiKeys = new List<ApiKeyModel>();
            foreach (var apiKey in apiKeysFromDB)
            {
                var item = new ApiKeyModel()
                {
                    Id = apiKey.Id,
                    Name = apiKey.Name,
                    Description = apiKey.Description,
                    IsEnabled = apiKey.IsEnabled,
                    ExpirationDateUtc = apiKey.ExpirationDateUtc,
                    UserId = apiKey.UserId,
                    ApiKey = _apiKeyService.GetDecryptedAPIKey(apiKey.EncryptedKey),
                    CreatedOnUtc = apiKey.CreatedOnUtc,
                };

                apiKeys.Add(item);
            }
            return apiKeys;
        }

        public async Task<ApiKeyModel> GetApiKeyModel(int id)
        {
            var apiKeyFromDB = await _apiKeyService.GetApiKeyByIdAsync(id);
            if (apiKeyFromDB == null)
                throw new Exception($"Can't find apiKey with id: {id}");

            var model = new ApiKeyModel()
            {
                Id = apiKeyFromDB.Id,
                Name = apiKeyFromDB.Name,
                Description = apiKeyFromDB.Description,
                IsEnabled = apiKeyFromDB.IsEnabled,
                ExpirationDateUtc = apiKeyFromDB.ExpirationDateUtc,
                UserId = apiKeyFromDB.UserId,
                ApiKey = _apiKeyService.GetDecryptedAPIKey(apiKeyFromDB.EncryptedKey),
                CreatedOnUtc = apiKeyFromDB.CreatedOnUtc,
            };

            return model;
        }

        public async Task CreateApiKey(ApiKeyModel model)
        {
            var apiKey = new ApiKey()
            {
                CreatedOnUtc = DateTime.UtcNow,
                Name = model.Name,
                Description = model.Description,
                IsEnabled = model.IsEnabled,
                ExpirationDateUtc = model.ExpirationDateUtc,
                UserId = model.UserId,
                EncryptedKey = _apiKeyService.GetEncryptedAPIKey(_apiKeyService.GenerateApiKey())
            };
            await _apiKeyService.InsertApiKeyAsync(apiKey);
        }

        public async Task UpdateApiKey(ApiKeyModel model)
        {
            if (model == null)
                throw new Exception("Incorrect model passed as a parameter.");

            var apiKeyFromDB = await _apiKeyService.GetApiKeyByIdAsync(model.Id);
            if (apiKeyFromDB == null)
                throw new Exception($"Can't find apiKey with id: {model.Id}");

            apiKeyFromDB.Name = model.Name;
            apiKeyFromDB.Description = model.Description;
            apiKeyFromDB.IsEnabled = model.IsEnabled;
            apiKeyFromDB.ExpirationDateUtc = model.ExpirationDateUtc;
            apiKeyFromDB.UserId = model.UserId;
            apiKeyFromDB.EncryptedKey = _apiKeyService.GetEncryptedAPIKey(model.ApiKey);

            await _apiKeyService.UpdateApiKeyAsync(apiKeyFromDB);
        }

        public async Task DeleteApiKey(int id)
        {
            var apiKeyFromDB = await _apiKeyService.GetApiKeyByIdAsync(id);
            if (apiKeyFromDB == null)
                throw new Exception($"Can't find apiKey with id: {id}");

            await _apiKeyService.DeleteApiKeyAsync(apiKeyFromDB);
        }


    }
}
