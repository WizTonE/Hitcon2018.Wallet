using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hitcon2018.Wallet
{

    public class TransferPageMenuItem
    {
        public TransferPageMenuItem()
        {
            TargetType = typeof(TransferPageDetail);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}