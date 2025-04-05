using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Checkout;
using Nop.Web.Validators.Common;

namespace Nop.Web.Validators.Checkout
{
    public partial class CheckoutCustomerInfoValidator : AbstractValidator<HohoOnePageCheckoutModel>
    {
        public CheckoutCustomerInfoValidator(ILocalizationService localizationService,
            CustomerSettings customerSettings, IHttpContextAccessor httpContextAccessor)
        {

            var httpContext = httpContextAccessor.HttpContext;
            var actionDescriptor = httpContext?.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (actionDescriptor != null)
            {
                if (actionDescriptor.ActionName == "HohoOpcLoadDeliveryDetails")
                {
                    RuleFor(checkoutModel => checkoutModel.CustomerInfo).SetValidator(new CustomerInfoValidator(localizationService, customerSettings));
                }

                if (actionDescriptor.ActionName == "HohoOpcPayOrder")
                {
                    When(model => model.CheckoutDetails.SendToCustomer, () =>
                    {
                        When(m => !m.CheckoutDetails.UseElectronicCertificate, () =>
                        {
                            RuleFor(m => m.RecipientInfo.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));
                            RuleFor(m => m.RecipientInfo.Address).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required"));
                        });
                    });

                    When(model => model.CheckoutDetails.SendToRecipient, () =>
                    {
                        RuleFor(m => m.RecipientInfo.FirstName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.FirstName.Required"));
                        RuleFor(m => m.RecipientInfo.LastName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.LastName.Required"));

                        When(m => m.CheckoutDetails.UseElectronicCertificate, () =>
                        {
                            RuleFor(m => m.RecipientInfo.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Email.Required"));
                            RuleFor(m => m.RecipientInfo.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
                        });

                        When(m => !m.CheckoutDetails.UseElectronicCertificate, () =>
                        {
                            RuleFor(m => m.RecipientInfo.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
                            RuleFor(m => m.RecipientInfo.PhoneNumber).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));

                            RuleFor(m => m.RecipientInfo.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));
                            RuleFor(m => m.RecipientInfo.Address).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required"));
                        });
                    });

                }
            }


        }
    }
}
