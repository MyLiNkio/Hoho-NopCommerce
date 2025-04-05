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
    public partial record ActivateCertificateModel : RedeemCertificateModel
    {
        #region User data

        public bool FirstNameEnabled { get; set; } = true;
        [NopResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }
        public bool FirstNameRequired { get; set; } = true;

        public bool LastNameEnabled { get; set; } = true;
        [NopResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }
        public bool LastNameRequired { get; set; } = true;

        public bool GenderEnabled { get; set; } = true;
        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }
        public bool GenderRequired { get; set; } = true;

        public bool DateOfBirthEnabled { get; set; } = true;
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }
        public bool DateOfBirthRequired { get; set; } = true;
        public DateTime? ParseDateOfBirth()
        {
            return CommonHelper.ParseDate(DateOfBirthYear, DateOfBirthMonth, DateOfBirthDay);
        }
        public void SetBirthdayDate(DateTime? date)
        {
            if (!date.HasValue)
                return;

            DateOfBirthDay = date.Value.Day;
            DateOfBirthMonth = date.Value.Month;
            DateOfBirthYear = date.Value.Year;
        }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Fields.Phone")]
        public string Phone { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        #endregion


        #region Activation details

        //Some wishes from user here
        [NopResourceDisplayName("Redeem.Fields.Comment")]
        public string Comment { get; set; }

        #endregion


        #region Agreements
        //agreements

        public bool AcceptTermsOfServiceEnabled { get; set; } = true;
        public bool TermsOfServicePopup { get; set; } = true;

        public bool AcceptServiceDescriptionRestrictionsEnabled { get; set; } = true;
        public bool ServiceDescriptionAccepted { get; set; } = true;

        public IList<GdprConsentModel> GdprConsents { get; set; } = new List<GdprConsentModel>();

        #endregion
    }
}