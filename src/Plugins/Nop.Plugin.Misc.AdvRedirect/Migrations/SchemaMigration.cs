using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AdvRedirect.Domain;

namespace Nop.Plugin.Misc.AdvRedirect.Migrations
{
    [NopMigration("2023/01/23 08:40:55", "Misc.AdvRedirect base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<RedirectionRule>();
        }
    }
}