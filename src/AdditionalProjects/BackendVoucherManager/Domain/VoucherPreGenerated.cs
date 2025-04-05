using System;
using Nop.Core;

namespace BackendVoucherManager.Domain
{
    public class VoucherPreGenerated : BaseEntity
    {
        public int Number { get; set; }

        public int? PartNumber { get; set; }
    }
}
