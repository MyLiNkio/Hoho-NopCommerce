using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.qBoSlider.Domain;

namespace Nop.Plugin.Widgets.qBoSlider.Migrations
{
    [NopMigration("2024/01/07 15:06:01:6455433", "Baroque. qBoSlider. Add Slide.UseOnMobileProperty", targetMigrationProcess: MigrationProcessType.Installation)]
    public partial class AddUseOnMobileMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if(!Schema.Table(nameof(Slide)).Column(nameof(Slide.UseOnMobile)).Exists())
                Create.Column(nameof(Slide.UseOnMobile)).OnTable(nameof(Slide)).AsBoolean().NotNullable().WithDefaultValue(0);

            if (!Schema.Table(nameof(WidgetZoneSlide)).Column(nameof(WidgetZoneSlide.UseOnMobile)).Exists())
                Create.Column(nameof(WidgetZoneSlide.UseOnMobile)).OnTable(nameof(WidgetZoneSlide)).AsBoolean().NotNullable().WithDefaultValue(0);
            
            if (!Schema.Table(nameof(WidgetZoneSlide)).Column(nameof(WidgetZoneSlide.MainOrChildSlideId)).Exists())
                Create.Column(nameof(WidgetZoneSlide.MainOrChildSlideId)).OnTable(nameof(WidgetZoneSlide)).AsInt32().Nullable().WithDefaultValue(null).Indexed();
        }
    }
}
