using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Customization.CertificatesManager.Mapping.Builders
{
    public partial class CertificateInfoBuilder : NopEntityBuilder<CertificateInfo>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            //.WithColumn(nameof(Certificate.Id)).AsInt32().PrimaryKey()
            .WithColumn(nameof(CertificateInfo.OrderId)).AsInt32().ForeignKey<Order>(onDelete: Rule.None)
            .WithColumn(nameof(CertificateInfo.PurchasedWithOrderItemId)).AsInt32().ForeignKey<OrderItem>(onDelete: Rule.None)
            .WithColumn(nameof(CertificateInfo.RedeemCustomerId)).AsInt32().Nullable().ForeignKey<CertificateRedeemCustomer>(onDelete: Rule.None)
            .WithColumn(nameof(CertificateInfo.Number)).AsInt32().Unique()
            .WithColumn(nameof(CertificateInfo.TypeId)).AsInt16()
            .WithColumn(nameof(CertificateInfo.StatusId)).AsInt16()
            .WithColumn(nameof(CertificateInfo.NominalPriceExclTax)).AsDecimal().NotNullable()
            .WithColumn(nameof(CertificateInfo.NominalPriceInclTax)).AsDecimal().NotNullable();
        }
    }
}
