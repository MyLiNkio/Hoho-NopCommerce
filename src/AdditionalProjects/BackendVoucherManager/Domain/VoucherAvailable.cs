using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendVoucherManager.Domain
{
    public class VoucherAvailable : VoucherPreGenerated
    {
        public int PartNumber { get; set; }

        public VoucherOriginIdentificator OriginId { get; set; }

        public int PinCode { get; set; }

        public string Encryption { get; set; }

        public long CreatedAtTicks { get; set; }

        public DateTime CreatedAt { get; set; }

        public int IncorrectValidationAttempts { get; set; } = 0;

        public DateTime? SoldAt { get; set; }

        public DateTime UpdatedAt { get; set;}
    }
}
