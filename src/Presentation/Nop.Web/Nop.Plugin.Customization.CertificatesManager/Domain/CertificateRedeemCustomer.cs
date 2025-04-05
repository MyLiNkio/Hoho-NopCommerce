using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Customization.CertificatesManager.Domain
{
    /// <summary>
    /// We collect that data to process it separately from registered users. 
    /// Currently there is no regulations in Georgia so we could create accounts automatically for users who activated the certificate and add difference to the balance
    /// But in future if it can become prohibited, so we should ask users and if they want to be registered. 
    /// If they refuse, we don't create account, but we still save user data to continue support providing untill certificate will be fully redeemed. And only after that we can remove personal user info
    /// </summary>
    public class CertificateRedeemCustomer : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public DateOnly Birthday { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime UpdatedAtUTC { get; set; }
    }
}
