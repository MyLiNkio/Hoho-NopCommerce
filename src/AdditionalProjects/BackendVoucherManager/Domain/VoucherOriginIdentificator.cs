using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendVoucherManager.Domain
{
    public enum VoucherOriginIdentificator
    {
        //10 - Hoho experience gift Phisical-vaucher
        //11 - Hoho experience gift Electonic-vaucher
        //20 - Hoho travel Phisical-vaucher
        //21 - Hoho travel Electronic-vaucher

        ExperienceGiftPhisical = 10,
        ExperienceGiftElectronic = 11,
        TravelPhisical = 20,
        TravelElectronic = 21,

        //ATTENTION!!!!! - the value can't be more than 99 as it is a firs part (XX) of certificate number: XX-00-00-00;

    }
}
