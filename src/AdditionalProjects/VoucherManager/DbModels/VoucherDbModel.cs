using System.ComponentModel.DataAnnotations.Schema;
using VoucherManager.Models;

namespace VoucherManager.Model
{
    [Table("Voucher")]
    internal class VoucherDbModel
    {
        public uint Id { get; set; }

        public ulong CardNumber { get; set; }

        public string? SecretCode { get; set; }

        /// <summary>
        /// Amount of attempts to activate voucher with incorrect security code
        /// </summary>
        public byte? InvalidValidationAttempts { get; set; }

        public byte OriginId { get; set; }

        /// <summary>
        /// URL with data which is codded in QR core
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Shows when the voucher was programmically generated
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date when voucher was sold
        /// </summary>
        public DateTime? SoldAt { get; set; }

        /// <summary>
        /// Voucher expiration date
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// The date till which vouched should be used
        /// </summary>
        public DateTime? RedeemTill { get; set; }
        
        /// <summary>
        /// The date when vouched was used
        /// </summary>
        public DateTime? RedeemedAt { get; set; }

        /// <summary>
        /// A date when voucher was blocked/suspended/canseled etc.
        /// </summary>
        public DateTime? BlockeddAt { get; set; }

        /// <summary>
        /// All logs will be added to that field regarding reason of blocking, rejecting and other useful information.
        /// </summary>
        public string? MessageLog { get; set; }

        /// <summary>
        /// Butch number. Shows the number of butch sent to print
        /// </summary>
        public ushort? PartNumber { get; set; }

        public VoucherStatus VoucherStatusId { get; set; }

        public long CreatedAtTicks { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
