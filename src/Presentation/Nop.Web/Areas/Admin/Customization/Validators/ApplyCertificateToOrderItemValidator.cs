using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Orders
{
    public partial class ApplyCertificateToOrderItemValidator : BaseNopValidator<ApplyCertificateToOrderItemModel>
    {
        public ApplyCertificateToOrderItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CertificateNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.Required"));
            RuleFor(x => x.CertificateNumber).Matches(@"^\d{2}-\d{2}-\d{2}-\d{2}$").WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.CNFormat"));

            //RuleFor(x => x.ValidityPeriod_Days).GreaterThanOrEqualTo(3).WithMessage("The validation period can't be less than 3 days. If you need less, please contact administrator.");

            RuleFor(x => x.ValidityPeriod_Days).Must((x, context) =>
            {
                var a = x;
                return x.ValidityPeriod_Days >= 3;
            }).WithMessage("The validation period can't be less than 3 days. If you need less, please contact administrator.");
        }
    }
}