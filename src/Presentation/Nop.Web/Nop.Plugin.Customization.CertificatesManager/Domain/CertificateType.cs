using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Customization.CertificatesManager.Domain
{
    public enum CertificateType
    {
        /// <summary>
        /// A standart electronic certificate where user can choose only one service from bounded choise
        /// </summary>
        StandartElectronic = 10,

        /// <summary>
        /// A standart physical card-certificate where user can choose only one service from bounded choise
        /// </summary>
        StandartPhysicalCard = 20,

        /// <summary>
        /// It's a certificate in electronic state with a nominal price but without any bounded services. So user can choose any service for that price value
        /// </summary>
        UniversalElectronic = 30,

        /// <summary>
        /// It's a physical card-certificate with a nominal price but without any bounded services. So user can choose any service for that price value
        /// </summary>
        UniversalPhysicalCard = 40,

        /// <summary>
        /// An electronic certificate with few bounded services and user can use all of them
        /// </summary>
        MegaElectronic = 50,

        /// <summary>
        /// A physical card-certificate with few bounded services and user can use all of them
        /// </summary>
        MegaPhysicalCard = 60,
    }
}
