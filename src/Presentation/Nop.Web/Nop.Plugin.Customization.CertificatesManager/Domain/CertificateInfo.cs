using System;
using Nop.Core;

namespace Nop.Plugin.Customization.CertificatesManager.Domain
{
    public class CertificateInfo : BaseEntity
    {
        public int OrderId { get; set; }

        public int PurchasedWithOrderItemId { get; set; }

        public int Number { get; set; }

        public CertificateType TypeId { get; set; }

        public decimal NominalPriceInclTax { get; set; }

        public decimal NominalPriceExclTax { get; set; }

        public CertificateStatus StatusId { get; set; }

        public DateTime SoldAtUTC { get; set; }

        public int ValidityDays { get; set; }

        public DateTime ExpiresAtUTC { get; set; }


        /// <summary>
        /// It is ID of record from table OrderItem. 
        /// 
        /// So basing on that record we are able to get all information about product including selected attributes.
        /// </summary>
        public int? ActivatedOrderItemId { get; set; }

        public int? RedeemCustomerId { get; set; }

        public DateTime? ActivatedAtUTC { get; set; }

        public DateTime? RedeemTillUTC { get; set; }

        public DateTime? RedeemedAtUTC { get; set; }

        public DateTime? BlockedAtUTC { get; set; }

        //TODO: we need a new table for prolongation as it will be somehow connected to new payment. New payment means new order. So we need a link to one more orderId
        //public DateTime? ProlongedAtUTC { get; set; }

        /// <summary>
        /// Gets or sets the record identifier
        /// </summary>
        public Guid CertificateGuid { get; set; }

        public DateTime UpdatedAtUTC { get; set; }
    }
}
