using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Builder;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using static LinqToDB.Sql;

namespace Nop.Plugin.Customization.CertificatesManager.Data
{
    [NopMigration("2024/07/12 09:00:12:1337541", "Nop.Plugin.Customization.CertificatesManager remake certificate tables", MigrationProcessType.Update)]
    public class RenameCertificateNotesTablesMigration : Migration
    {
        public override void Up()
        {
            //if (Schema.Table("CertificateNotes").Exists()
            //    && !Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Exists())
            //    Rename.Table("CertificateNotes").To(NameCompatibilityManager.GetTableName(typeof(CertificateNote)));

            //if (Schema.Table("certificateredeem").Exists())
            //    Delete.Table("certificateredeem");

            //if (Schema.Table("Certificate").Exists()
            //    && !Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Exists())
            //    Rename.Table("Certificate").To(NameCompatibilityManager.GetTableName(typeof(CertificateInfo)));

            //Execute.Sql("DELETE FROM localestringresource WHERE ResourceName LIKE '%Redeem.Activate.Temp%'");

            //if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Exists() &&
            //    Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Column("NoteTypeIdentifier").Exists())
            //{
            //    Rename.Column("NoteTypeIdentifier").
            //        OnTable(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).
            //        To(nameof(CertificateNote.NoteType));

            //    Alter.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote)))
            //        .AlterColumn(nameof(CertificateNote.NoteType)).AsInt16().NotNullable();
            //}

            //if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Exists() &&
            //    Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).Column("NoteMessage").Exists())
            //    Rename.Column("NoteMessage").
            //        OnTable(NameCompatibilityManager.GetTableName(typeof(CertificateNote))).
            //        To(nameof(CertificateNote.Message));


            //if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Exists() &&
            //    Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Column("ActivatedProductId").Exists())
            //    Rename.Column("ActivatedProductId").
            //        OnTable(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).
            //        To(nameof(CertificateInfo.ActivatedOrderItemId));

            //if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Exists() &&
            //    Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Column("ActivatedByCustomerId").Exists())
            //    Rename.Column("ActivatedByCustomerId").
            //        OnTable(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).
            //        To(nameof(CertificateInfo.RedeemCustomerId));


            //if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateRedeemCustomer))).Exists())
            //    Create.TableFor<CertificateRedeemCustomer>();

            //if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Exists() &&
            //    !Schema.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo))).Column(nameof(CertificateInfo.RedeemCustomerId)).Exists())
            //    Alter.Table(NameCompatibilityManager.GetTableName(typeof(CertificateInfo)))
            //        .AlterColumn(nameof(CertificateInfo.RedeemCustomerId)).AsInt32().Nullable()
            //        .ForeignKey<CertificateRedeemCustomer>().OnDelete(Rule.None);

        }

        public override void Down()
        {
        }
    }
}