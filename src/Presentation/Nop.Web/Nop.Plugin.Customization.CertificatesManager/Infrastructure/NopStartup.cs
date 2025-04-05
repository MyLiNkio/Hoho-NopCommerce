﻿using Nop.Core.Infrastructure;
using Nop.Plugin.Customization.CertificatesManager.Services;

namespace Nop.Plugin.Customization.CertificatesManager.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public partial class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<ICertificateItemService, CertificateItemService>();
            services.AddScoped<IMulticertificateAttributeService, MulticertificateAttributeService>();
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
        public int Order => 300;
    }
}