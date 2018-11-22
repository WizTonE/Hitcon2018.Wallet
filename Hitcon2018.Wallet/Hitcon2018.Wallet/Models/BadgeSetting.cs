using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class BadgeSetting
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string AESKey { get; set; }
        public string Address { get; set; }
        public string ServiceUUID { get; set; }
        public string Characteristic { get; set; }
    }
}
