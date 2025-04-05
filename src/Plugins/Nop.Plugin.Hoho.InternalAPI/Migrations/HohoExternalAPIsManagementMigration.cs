using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;

namespace Nop.Plugin.Hoho.ExternalAPIs.Migrations
{
    [NopMigration("2024/09/04 22:00:00", "Hoho.ExternalAPIsPlugin", MigrationProcessType.Installation)]
    public partial class HohoExternalAPIsManagementMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ApiKey))).Exists())
                Create.TableFor<ApiKey>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ApiRequestLog))).Exists())
                Create.TableFor<ApiRequestLog>();
        }
    }
}
