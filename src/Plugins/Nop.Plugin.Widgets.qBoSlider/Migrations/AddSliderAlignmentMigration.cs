using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.qBoSlider.Domain;

namespace Nop.Plugin.Widgets.qBoSlider.Migrations
{
    [NopMigration("2024/01/07 15:06:00:6455432", "Baroque. qBoSlider. Add slider alignment property for widget zones", MigrationProcessType.Installation)]
    public partial class AddSliderAlignmentMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(nameof(WidgetZone)).Column(nameof(WidgetZone.SliderAlignmentId)).Exists())
            { 
                Create.Column(nameof(WidgetZone.SliderAlignmentId)).OnTable(nameof(WidgetZone)).AsInt32().NotNullable().WithDefaultValue(5);
            }
        }
    }
}
