using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Extensions;
using System.Data;
using Nop.Plugin.Customization.CertificatesManager.Domain;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Customization.CertificatesManager.Mapping.Builders
{
    internal class CertificateNoteBuilder : NopEntityBuilder<CertificateNote>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            //.WithColumn(nameof(CertificateNote.Id)).AsInt32().PrimaryKey()
            .WithColumn(nameof(CertificateNote.CertificateId)).AsInt32().ForeignKey<CertificateInfo>(onDelete: Rule.Cascade)
            .WithColumn(nameof(CertificateNote.NoteType)).AsInt16();
        }
    }
}
