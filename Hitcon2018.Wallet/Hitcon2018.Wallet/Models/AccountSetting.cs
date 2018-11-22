using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class AccountSetting
    {
        [PrimaryKey]
        public string Address { get; set; }
        public string AccountName { get; set; }
        public string Amount { get; set; }
        public string PrivateKey { get; set; }
        public int Type { get; set; } //0: Soft Wallet, 1: Badge Wallet
        public bool isBadge { get => Type == 1; }
        public bool isSoftAccount { get => Type == 0; }
    }
}
