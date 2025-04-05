using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Hoho.ExternalAPIs.Domain;

namespace Nop.Plugin.Hoho.ExternalAPIs.Mapping.Builders
{
    public partial class ApiRequestLogBuilder : NopEntityBuilder<ApiRequestLog>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ApiRequestLog.ApiKeyId)).AsInt32().NotNullable()
                .WithColumn(nameof(ApiRequestLog.UserId)).AsInt32().Nullable()
                .WithColumn(nameof(ApiRequestLog.RequestUrl)).AsString(1024).NotNullable()
                .WithColumn(nameof(ApiRequestLog.RequestMethod)).AsString(10).NotNullable()
                .WithColumn(nameof(ApiRequestLog.IpAddress)).AsString(45).NotNullable() // IPv6 поддержка
                .WithColumn(nameof(ApiRequestLog.UserAgent)).AsString(1024).Nullable()
                .WithColumn(nameof(ApiRequestLog.RequestTimeUtc)).AsDateTime().NotNullable();
        }
    }
}
