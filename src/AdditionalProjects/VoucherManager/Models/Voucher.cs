using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoucherManager.Models
{
    public class Voucher
    {
        public ulong CardNumber { get; set; }
        
        public string QRCodeData { get; set; }

        public bool? SecurityValidationPassed { get; set; }

        public byte? InvalidValidationAttempts { get; set; }

        public int MaxInvalidValidationAttempts { get; set; }

        public byte OriginId { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? RedeemTill { get; set; }

        public DateTime? SoldAt { get; set; }

        public DateTime? RedeemedAt { get; set; }

        public DateTime? BlockedAt { get; set; }

        public VoucherStatus VoucherStatus { get; set; }
    }
}
