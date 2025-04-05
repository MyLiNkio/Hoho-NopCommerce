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
    public partial record RedeemCertificateModel : CheckCertificateModel
    {
        public int? OrderItemId { get; set; }

        public List<RedeemOrderItemModel> OrderItems { get; set; } = new List<RedeemOrderItemModel>();
    }

    public partial record RedeemOrderItemModel : ProductDetailsModel
    {
        #region Cstr
        public RedeemOrderItemModel(int id, ProductDetailsModel baseModel):base(baseModel)
        {
            OrderItemId = id;
        }

        #endregion

        public int OrderItemId { get; set; }
    }
}