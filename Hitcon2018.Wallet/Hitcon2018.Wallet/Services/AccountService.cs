using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class AccountService
    {
        private static AccountService _accountService;
        public static AccountService Instance { get { return _accountService ?? (_accountService = new AccountService()); } }
        public AccountSetting AccountSetting { get; set; }
        private AccountService()
        {

        }
    }
}
