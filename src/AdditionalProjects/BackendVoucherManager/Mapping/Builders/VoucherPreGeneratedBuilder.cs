using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using BackendVoucherManager.Domain;
using Nop.Core.Domain.Orders;

namespace BackendVoucherManager.Mapping.Builders
{
    public partial class VoucherPreGeneratedBuilder : NopEntityBuilder<VoucherPreGenerated>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            .WithColumn(nameof(VoucherPreGenerated.Number)).AsInt32().Unique().NotNullable()
            .WithColumn(nameof(VoucherPreGenerated.PartNumber)).AsInt32().Nullable();
        }
    }
}
