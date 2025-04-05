using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using CsvHelper.Configuration.Attributes;
using BackendVoucherManager.Domain;

namespace BackendVoucherManager.Models
{
    public class AvailableVoucherDataModel
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public VoucherOriginIdentificator OriginId { get; set; }

        public int IncorrectValidationAttempts { get; set; } = 0;

        public DateTime? SoldAt { get; set; }
    }
}
