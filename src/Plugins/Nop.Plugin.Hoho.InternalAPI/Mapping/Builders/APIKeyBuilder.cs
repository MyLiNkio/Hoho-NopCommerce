using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;

namespace Nop.Plugin.Hoho.ExternalAPIs.Mapping.Builders
{
    public partial class APIKeyBuilder : NopEntityBuilder<ApiKey>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ApiKey.Name)).AsString(100).NotNullable()
                .WithColumn(nameof(ApiKey.Description)).AsString(255).Nullable()
                .WithColumn(nameof(ApiKey.EncryptedKey)).AsString(255).NotNullable()
                .WithColumn(nameof(ApiKey.IsEnabled)).AsBoolean().NotNullable()
                .WithColumn(nameof(ApiKey.UserId)).AsInt32().Nullable()
                .WithColumn(nameof(ApiKey.ExpirationDateUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(ApiKey.CreatedOnUtc)).AsDateTime().NotNullable();
        }
    }
}
