using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Models.Checkout
{
    public partial record HohoOnePageCheckoutModel : BaseNopModel
    {
        public decimal TotalSum { get; set; }
        public decimal CertificateNominalPrice { get; set; }
        public decimal PackagingPrice { get; set; }
        public decimal ShippingCost { get; set; }

        public CheckoutDetailsModel CheckoutDetails { get; set; } = new CheckoutDetailsModel();
        public CheckoutCustomerModel CustomerInfo { get; set; }
        public CheckoutRecipientModel RecipientInfo { get; set; }


        public CheckoutShippingMethodModel ShippingMethods { get; set; }
        public CheckoutPaymentMethodModel PaymentMethods { get; set; }

        public bool ShippingRequired { get; set; }
        public bool DisableBillingAddressCheckoutStep { get; set; }
        public bool DisplayCaptcha { get; set; }
        public bool IsReCaptchaV3 { get; set; }
        public string ReCaptchaPublicKey { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }
    }

    public record CheckoutDetailsModel: BaseNopModel
    {
        public bool UseElectronicCertificate { get; set; } = false;
        public bool BuyingAsGift { get; set; } = true;

        public bool PickupInStore { get; set; }
        public bool SendToCustomer { get; set; }
        public bool SendToRecipient { get; set; }

        public int? BoxProductId { get; set; }
        public List<ProductOverviewModel> PackagingProducts { get; set; }
        public bool UseCongratulationText { get; set; } = false;
        public string CongratulationText { get; set; }

        public bool SendAnonymously { get; set; }
        public bool DeliverByExactTime { get; set; } = false;
        public DateTime DeliveryDate { get; set; }
        public DateTime DeliveryTime { get; set; }
    }

    public record CheckoutCustomerModel: BaseNopModel
    {
        [NopResourceDisplayName("Address.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Address.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Address.Fields.Email")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Address.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public DateTime BirthdayDate { get; set; }
    }

    public record CheckoutRecipientModel : CheckoutCustomerModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Account.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Account.Fields.StreetAddress")]
        public string Address { get; set; }
    }
}