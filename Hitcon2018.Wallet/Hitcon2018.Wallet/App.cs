using System;
using System.Collections.Generic;
using System.Text;
using Plugin.BluetoothLE;
using Xamarin.Forms;

namespace Hitcon2018.Wallet
{
    public partial class App : Application
    {
        public App()
        {
            this.MainPage = CrossBleAdapter.AdapterScanner.IsSupported
                ? new NavigationPage(new AdapterListPage())
                : new NavigationPage(new AdapterPage());
        }
    }
}
