using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using BackendVoucherManager.Domain;
using Nop.Core.Domain.Orders;

namespace BackendVoucherManager.Mapping.Builders
{
    public partial class VoucherAvailableBuilder : NopEntityBuilder<VoucherAvailable>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            .WithColumn(nameof(VoucherAvailable.Number)).AsInt32().Unique().NotNullable()
            .WithColumn(nameof(VoucherAvailable.OriginId)).AsInt32().NotNullable()
            .WithColumn(nameof(VoucherAvailable.PinCode)).AsInt32().NotNullable()
            .WithColumn(nameof(VoucherAvailable.Encryption)).AsString(100).NotNullable();
        }
    }
}
