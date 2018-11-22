using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class Transaction
    {
        [PrimaryKey]
        public string Address { get; set; }
        public string FromAddress { get; set; }
        public int Type { get; set; }
        public string Amount { get; set; }
        public string GasLimit { get; set; }
        public string GasPrice { get; set; }
        public int Status { get; set; }
        public DateTime Date { get; set; }
    }
}
