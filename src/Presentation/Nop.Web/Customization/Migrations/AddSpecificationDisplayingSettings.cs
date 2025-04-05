using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Customization.CertificatesManager.Domain;

namespace Nop.Plugin.Customization.CertificatesManager.Data
{
    [NopMigration("2024/12/12 10:51:01:1337542", "HohoCustomization add information about specification attributes displaying", MigrationProcessType.NoMatter)]
    public class AddSpecificationDisplayingSettings : Migration
    {
        public override void Up()
        {
            Delete.FromTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new { Name = "TopMenu.DisplaySpecificationAttr" });
            Insert.IntoTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new
            {
                Name = "TopMenu.DisplaySpecificationAttr",
                Value = "10,8",
                StoreId = 0
            });


            Delete.FromTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new { Name = "TopMenu.BaseCategoryIdToDisplaySpecificationAttr" });
            Insert.IntoTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new
            {
                Name = "TopMenu.BaseCategoryIdToDisplaySpecificationAttr",
                Value = "24",
                StoreId = 0
            });


            Delete.FromTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new { Name = "TopMenu.DisplayServiceLangSpecificationAttr" });
            Insert.IntoTable(NameCompatibilityManager.GetTableName(typeof(Setting))).Row(new
            {
                Name = "TopMenu.DisplayServiceLangSpecificationAttr",
                Value = "9",
                StoreId = 0
            });
        }

        public override void Down()
        {
            //add revert code if required.
        }

    }
}