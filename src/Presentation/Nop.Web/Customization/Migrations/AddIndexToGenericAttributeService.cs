using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Customization.CertificatesManager.Domain;

namespace Nop.Plugin.Customization.CertificatesManager.Data
{
    [NopMigration("2024/01/20 14:51:01:1337541", "HohoCustomization add index to genericAttribute existing column", MigrationProcessType.NoMatter)]
    public class AddIndexToGenericAttributeService : AutoReversingMigration
    {
        public override void Up()
        {
            //Create.Index("IX_GenericAttribute_EntityId_and_KeyGroup").OnTable(nameof(GenericAttribute))
            //    .OnColumn(nameof(GenericAttribute.EntityId)).Ascending()
            //    .OnColumn(nameof(GenericAttribute.KeyGroup)).Ascending()
            //    .WithOptions().NonClustered();
        }
    }
}