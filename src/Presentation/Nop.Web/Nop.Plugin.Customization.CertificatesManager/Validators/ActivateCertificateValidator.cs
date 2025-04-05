using System;
using BackendVoucherManager.Services;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Customization.CertificatesManager.Models;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Customization.CertificatesManager.Validators
{
    public partial class ActivateCertificateValidator : BaseNopValidator<ActivateCertificateModel>
    {
        public ActivateCertificateValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IBaseVoucherService baseVoucherService,
            CustomerSettings customerSettings)
        {
            RuleFor(x => x.CertificateNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.Required"));
            RuleFor(x => x.CertificateNumber).Matches(@"^\d{2}-\d{2}-\d{2}-\d{2}$").WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.CNFormat"));

            RuleFor(x => x.SecurityCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.Required"));
            RuleFor(x => x.SecurityCode).Matches(@"^\d{2}-\d{2}$").WithMessageAwait(localizationService.GetResourceAsync("Redeem.Fields.Valid.SecurityFormat"));

            RuleFor(x => x.SecurityCode).Must((x, context) =>
            {
                if (string.IsNullOrEmpty(x.SecurityCode))
                    return false;
                return baseVoucherService.ValidateVoucherByPassword(x.CertificateNumber, x.SecurityCode).Result;

                //TODO: add password validation
                //var vInfo = VouchersManager.ValidateVoucherAsync(x.CertificateNumber, x.SecurityCode).Result;
                //if (vInfo == null)
                //    return false;

                //x.MaxInvalidValidationAttempts = vInfo.MaxInvalidValidationAttempts;
                //x.InvalidValidationAtemptsAmount = vInfo.InvalidValidationAttempts;
                //x.SecurityValidationPassed = vInfo.SecurityValidationPassed;

                if (x.SecurityValidationPassed.HasValue && x.SecurityValidationPassed.Value == false)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Redeem.IncorrectSecurityCode"));


            RuleFor(x => x.FirstName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.FirstName.Required"));
            RuleFor(x => x.LastName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.LastName.Required"));

            RuleFor(x => x.Phone).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
            RuleFor(x => x.Phone).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));

            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));

            RuleFor(x => x.DateOfBirthDay).Must((x, context) =>
            {
                if (!x.DateOfBirthEnabled || !x.DateOfBirthRequired)
                    return true;

                var dateOfBirth = x.ParseDateOfBirth();
                if (!dateOfBirth.HasValue)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.Required"));

            //minimum age
            RuleFor(x => x.DateOfBirthDay).Must((x, context) =>
            {
                if (!x.DateOfBirthEnabled || !x.DateOfBirthRequired)
                    return true;

                var dateOfBirth = x.ParseDateOfBirth();
                if (dateOfBirth.HasValue && customerSettings.DateOfBirthMinimumAge.HasValue &&
                    CommonHelper.GetDifferenceInYears(dateOfBirth.Value, DateTime.Today) <
                    customerSettings.DateOfBirthMinimumAge.Value)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.MinimumAge"), customerSettings.DateOfBirthMinimumAge);

            RuleFor(x => x.Gender).Must((x, context) =>
            {
                if (!x.GenderEnabled || !x.GenderRequired)
                    return true;

                var dateOfBirth = x.ParseDateOfBirth();
                if (x.Gender.IsNullOrEmpty())
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.Required"));

        }
    }
}