using BackendVoucherManager.Domain;
using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;

namespace BackendVoucherManager.Migrations
{
    [NopSchemaMigration("2023/12/16 00:01:02", "BackendVoucherManager creates basic Voucher controlling tables2", MigrationProcessType.NoMatter)]
    public class VouchersControlTablesMigration : AutoReversingMigration
    {
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(VoucherPreGenerated))).Exists())
                Create.TableFor<VoucherPreGenerated>();

            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(VoucherAvailable))).Exists())
                Create.TableFor<VoucherAvailable>();
        }
    }
}