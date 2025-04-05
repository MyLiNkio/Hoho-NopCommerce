using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.AdvRedirect.Domain;

namespace Nop.Plugin.Misc.AdvRedirect.Data
{
    [NopMigration("2023/01/10 09:09:17:6455442", "Misc.AdvRedirect base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
	        if(!Schema.Table("RedirectionRule").Exists())
			    Create.TableFor<RedirectionRule>();
        }

        #endregion
    }
}