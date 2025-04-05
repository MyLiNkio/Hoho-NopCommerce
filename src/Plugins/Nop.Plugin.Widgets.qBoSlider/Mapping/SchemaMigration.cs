using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.qBoSlider.Domain;

namespace Nop.Plugin.Widgets.qBoSlider.Mapping
{
    //[SkipMigrationOnUpdate]
    [NopMigration("2024/01/07 15:06:00:1687541", "Baroque.Widgets.qBoSlider base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(WidgetZone))).Exists())
                Create.TableFor<WidgetZone>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Slide))).Exists())
                Create.TableFor<Slide>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(WidgetZoneSlide))).Exists())
                Create.TableFor<WidgetZoneSlide>();
        }
    }
}
