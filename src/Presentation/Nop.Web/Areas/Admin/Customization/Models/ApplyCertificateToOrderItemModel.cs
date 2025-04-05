using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents a product model to add to the order
    /// </summary>
    public partial record ApplyCertificateToOrderItemModel : BaseNopModel
    {
        #region Ctor

        public ApplyCertificateToOrderItemModel()
        {
            Warnings = new List<string>();
        }

        #endregion


        #region Properties

        public int OrderId { get; set; }

        public int OrderItemId { get; set; }

        public string CertificateNumber { get; set; }

        public int ValidityPeriod_Days { get; set; }

        public List<string> Warnings { get; set; }

        public bool HasCertificateApplied { get; set; }

        #endregion
    }
}