using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Customization;

namespace Nop.Web.Infrastructure;

/// <summary>
/// Represents the registering services on application startup
/// </summary>
public partial class NopHohoCustomizationStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //Overriden services
        services.AddScoped<IOrderTotalCalculationService, OrderTotalCalculationServiceOverridden>();
        services.AddScoped<IOrderTotalCalculationServiceOverridden, OrderTotalCalculationServiceOverridden>();
        services.AddScoped<IOrderProcessingService, OrderProcessingServiceOverridden>();
        services.AddScoped<IMessageTokenProvider, MessageTokenProviderOverridden>();
        services.AddScoped<IPdfService, PdfServiceOverridden>();
        services.AddScoped<IProductAttributeFormatter, ProductAttributeFormatterCustomized>();
        services.AddScoped<IProductAttributeFormatterCustomized, ProductAttributeFormatterCustomized>();
        services.AddScoped<IMakeWebhookService, MakeWebhookService>();
        services.AddScoped<INewOrderNotificationService, NewOrderNotificationService>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 999999;
}
