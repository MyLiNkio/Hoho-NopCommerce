

namespace Nop.Plugin.Customization.CertificatesManager.Models
{
    public enum CertificateDisplayStatus
    {
        /// <summary>
        /// We can't find this number of certificate. Please check the number carefully and try again if there is a mistake
        /// </summary>
        NotFound = 10,

        /// <summary>
        /// Certificate is blocked to use due to suspicious operations or other reason
        /// </summary>
        Blocked = 20,

        /// <summary>
        /// We are sorry, but this certificate expired at {date}
        /// </summary>
        Expired = 30,

        /// <summary>
        /// That Certificate is already redeemed
        /// </summary>
        Redeemed = 40,

        /// <summary>
        /// This certificate is valid till {date}
        /// </summary>
        Valid = 50,

        /// <summary>
        /// That certificate is activated and should be redeemed till {date}
        /// </summary>
        Activated = 60,
    }
}
