using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Customization.CertificatesManager.Domain;

namespace Nop.Plugin.Customization.CertificatesManager.Data
{
    [NopSchemaMigration("2024/07/12 14:01:08:1337543", "Nop.Plugin.Customization.CertificatesManager base schema2", MigrationProcessType.NoMatter)]
    public class CreateCertificateTablesMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateRedeemCustomer))).Exists())
                Create.TableFor<CertificateRedeemCustomer>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Exists())
                Create.TableFor<CertificateInfo>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Exists())
                Create.TableFor<CertificateNote>();
        }
    }
}