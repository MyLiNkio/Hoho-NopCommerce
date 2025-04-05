using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoucherManager.Models
{
    public enum OriginIdentificator
    {
        //10 - Hoho experience gift Phisical-vaucher
        //11 - Hoho experience gift Electonic-vaucher
        //20 - Hoho travel Phisical-vaucher
        //21 - Hoho travel Electronic-vaucher

        ExperienceGiftPhisical = 10,
        ExperienceGiftElectronic = 11,
        TravelPhisical = 20,
        TravelElectronic = 21,
    }
}
