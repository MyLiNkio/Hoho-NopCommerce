using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Common;
using Nop.Services.Plugins;

namespace Nop.Plugin.Customization.CertificatesManager
{
    public class CertificatesPlugin: BasePlugin, IMiscPlugin
    {


        //https://docs.nopcommerce.com/en/developer/plugins/how-to-write-plugin-4.60.html
        //        Then for each plugin that has a configuration page, you should specify a configuration URL.Base class named BasePlugin has GetConfigurationPageUrl method which returns a configuration URL:

        //public override string GetConfigurationPageUrl()
        //        {
        //            return $"{_webHelper.GetStoreLocation()}Admin/{CONTROLLER_NAME}/{ACTION_NAME}";
        //        }
        //        Where {CONTROLLER_NAME
        //    } is the name of your controller and { ACTION_NAME } is the name of the action(usually it's Configure).


        //        Once you have installed your plugin and added the configuration method you will find a link to configure your plugin under Admin → Configuration → Local Plugins.
    }
}
