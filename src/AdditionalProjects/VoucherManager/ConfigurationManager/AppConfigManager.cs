using System;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace VoucherManager.ConfigurationManager
{
    class AppConfigManager
    {
        #region Initialization

        private AppConfigManager() {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile($"appsettings.json", true, true);
            Config = builder.Build();
        }

        private static AppConfigManager _instance;

        private static readonly object _lock = new object();

        public static AppConfigManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppConfigManager();
                    }
                }
            }
            return _instance;
        }

        private IConfiguration Config { get; set; }

        #endregion Initialization

        #region Public methods

        public T GetSection<T>(string sectionKey)
        {
            var section = Config.GetSection(sectionKey);
            return section.Get<T>();
        }

        public string GetSection(string sectionKey)
        {
            return Config[sectionKey];
        }


        #endregion

    }
}
