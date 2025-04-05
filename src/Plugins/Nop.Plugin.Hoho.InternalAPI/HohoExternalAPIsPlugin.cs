using Nop.Services.Common;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Hoho.InternalAPI
{
    public class HohoExternalAPIsPlugin : BasePlugin, IMiscPlugin
    {
        private readonly IPermissionService _permissionService;

        public HohoExternalAPIsPlugin(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public override async Task InstallAsync()
        {
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
        }
    }
}
