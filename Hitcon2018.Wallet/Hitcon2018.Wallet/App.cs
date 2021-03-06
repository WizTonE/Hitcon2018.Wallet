﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Plugin.BluetoothLE;
using Xamarin.Forms;

namespace Hitcon2018.Wallet
{
    public partial class App : Application
    {
        static WalletDatabase database;

        public static WalletDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new WalletDatabase(
                      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TodoSQLite.db3"));
                }
                return database;
            }
        }

        public App()
        {
            //this.MainPage = CrossBleAdapter.AdapterScanner.IsSupported
            //    ? new NavigationPage(new AdapterListPage())
            //    : new NavigationPage(new AdapterPage());
            this.MainPage = new NavigationPage(new SplashPage());
        }
    }
}
