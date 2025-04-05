using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Customization.CertificatesManager.Mapping.Builders
{
    public partial class CertificateRedeemCustomerBuilder : NopEntityBuilder<CertificateRedeemCustomer>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            //.WithColumn(nameof(CertificateRedeem.Id)).AsInt32().PrimaryKey()
            .WithColumn(nameof(CertificateRedeemCustomer.FirstName)).AsString(255).NotNullable()
            .WithColumn(nameof(CertificateRedeemCustomer.LastName)).AsString(255).NotNullable()
            .WithColumn(nameof(CertificateRedeemCustomer.Gender)).AsString(20)
            .WithColumn(nameof(CertificateRedeemCustomer.Birthday)).AsDate().NotNullable()
            .WithColumn(nameof(CertificateRedeemCustomer.Email)).AsString(255).NotNullable()
            .WithColumn(nameof(CertificateRedeemCustomer.PhoneNumber)).AsString(20).NotNullable()
            .WithColumn(nameof(CertificateRedeemCustomer.UpdatedAtUTC)).AsDateTime().NotNullable();
        }
    }
}
