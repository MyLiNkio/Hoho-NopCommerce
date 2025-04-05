using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Customer;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Customization.CertificatesManager.Models
{
    public partial record CheckCertificateModel : BaseNopModel
    {
        [NopResourceDisplayName("Redeem.Fields.CardNumber")]
        public string CertificateNumber { get; set; }

        [NopResourceDisplayName("Redeem.Fields.SecurityCode")]
        public string SecurityCode { get; set; }

        public string ValidationToken { get; set; }

        public bool? SecurityValidationPassed { get; set; }
        public int? InvalidValidationAtemptsAmount { get; set; }
        public int MaxInvalidValidationAttempts { get; set; }
        public CertificateDisplayStatus? Status { get; set; }
        public DateTime? ValidationDate { get; set; }

        //public DateTime SoldAt { get; set; }
        //public DateTime ExpiresAt { get; set; }
        //public DateTime? ActivatedAt { get; set; }
        //public DateTime? RedeemedAt { get; set; }
        //public DateTime? RedeemTill { get; set; }
        //public DateTime? BlockedAt { get; set; }

    }
}