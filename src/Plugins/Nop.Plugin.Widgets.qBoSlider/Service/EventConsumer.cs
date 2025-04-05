using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.qBoSlider.Service;

/// <summary>
/// Represents the plugin event consumer
/// </summary>
public class EventConsumer : BaseAdminMenuCreatedEventConsumer
{
    #region Fields

    private readonly IAdminMenu _adminMenu;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IShipmentService _shipmentService;
    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public EventConsumer(IAdminMenu adminMenu,
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IShipmentService shipmentService,
        IPermissionService permissionService,
        IPluginManager<IPlugin> pluginManager,
        IWorkContext workContext, IWebHelper webHelper) : base(pluginManager)
    {
        _adminMenu = adminMenu;
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _shipmentService = shipmentService;
        _permissionService = permissionService;
        _workContext = workContext;
        _webHelper = webHelper;
    }

    #endregion

    #region Utitites

    /// <summary>
    /// Checks is the current customer has rights to access this menu item
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the true if access is granted, otherwise false
    /// </returns>
    protected override async Task<bool> CheckAccessAsync()
    {
        return await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS);
    }

    /// <summary>
    /// Gets the menu item
    /// </summary>
    /// <param name="plugin">The instance of <see cref="IPlugin"/> interface</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the instance of <see cref="AdminMenuItem"/>
    /// </returns>
    protected override async Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
    {
        var descriptor = plugin.PluginDescriptor;

        return new AdminMenuItem
        {
            Visible = true,
            //SystemName = "Baroque-qboSlider",
            SystemName = descriptor.SystemName,
            //Title = descriptor.FriendlyName,
            Title = await _localizationService.GetLocalizedFriendlyNameAsync(plugin, (await _workContext.GetWorkingLanguageAsync()).Id),
            IconClass = "far fa-dot-circle",
            ChildNodes = new List<AdminMenuItem>
            {
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}-WidgetZone",
                    Title = $"WidgetZone",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/qBoWidgetZone/List",
                    IconClass = "fas fa-circle"
                },
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}-Slides",
                    Title = $"Slides",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/qBoSlide/List",
                    IconClass = "fas fa-circle"
                },
                new()
                {
                    Visible = true,
                    SystemName = $"{descriptor.SystemName}-Configuration",
                    Title = $"Configuration",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/qBoConfiguration/Configure",
                    IconClass = "fas fa-circle"
                }
            }
        };
    }
    
    #endregion

    #region Methods

    #endregion

    #region Properties

    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    protected override string PluginSystemName => qBoSliderDefaults.SystemName;

    /// <summary>
    /// Menu item insertion type
    /// </summary>
    protected override MenuItemInsertType InsertType => MenuItemInsertType.TryAfterThanBefore;

    /// <summary>
    /// The system name of the menu item after with need to insert the current one
    /// </summary>
    protected override string AfterMenuSystemName => "Third party plugins";

    /// <summary>
    /// The system name of the menu item before with need to insert the current one
    /// </summary>
    protected override string BeforeMenuSystemName => "Local plugins";

    #endregion
}