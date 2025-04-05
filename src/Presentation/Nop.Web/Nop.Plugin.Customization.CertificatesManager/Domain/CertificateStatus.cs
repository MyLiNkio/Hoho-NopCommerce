using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Customization.CertificatesManager.Domain
{
    public enum CertificateStatus
    {
        Sold = 10, //Certificate is sold
        Activated = 20, //The Certificate was activated
        Redeemed = 30, //The Certificate was redeemed after activation because service provider confirmed it
        PaidOut = 40, //The company has paid money to service provider for that Certificate
        Expired = 50, //The Certificate is expired
        Canceled = 60, //The Certificate was canceled because customer returned it or divided to other two Certificates
        Blocked = 70, //Due to un suspicient actions or other reason
    }
}
