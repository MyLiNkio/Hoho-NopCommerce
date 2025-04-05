using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Plugin.Misc.AdvRedirect.Domain;
using Nop.Plugin.Misc.AdvRedirect.Enums;
using Nop.Plugin.Misc.AdvRedirect.Models.Redirections;

namespace Nop.Plugin.Misc.AdvRedirect.Services
{
    public interface IRedirectionsService
    {
        void DeleteRedirectionAsync(RedirectionRule ent);

        Task<IPagedList<RedirectionRule>> GetAllRedirectionsAsync(RedirectionSearchModel searchModel);


        Task<InsertRedirectionResult> InsertRedirectionsAsync(RedirectionRule ent);

        Task<string> ResolveRedirection(HttpRequest request);
    }
}