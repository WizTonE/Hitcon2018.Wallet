using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{

    public class QRcodeParingViewModel : ViewModel
    {
        public string Address { get; set; }
        public string AESkey { get; set; }
        public string ServiceUUID { get; set; }
        public string Characteristics { get; set; }
    }
}
