using Plugin.BluetoothLE;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Hitcon2018.Wallet
{
    public class EthViewModel : ViewModel
    {
        private EthService ethService;
        private string address;
        public EthViewModel()
        {
            //BLEMatching = new Command(() => new NavigationPage(new AdapterPage()));
            BLEMatching = ReactiveCommand.Create(async () => await App.Current.MainPage.Navigation.PushAsync(new AdapterPage()));
            GetTotalBalance = ReactiveCommand.Create(()=> ethService.GetBalance(address));
            Transfer = ReactiveCommand.Create(async () => await App.Current.MainPage.Navigation.PushAsync(new TransferPage()));
            ShowAccount = ReactiveCommand.Create(async () => await App.Current.MainPage.Navigation.PushAsync(new WalletPage()));
        }

        public ICommand GetTotalBalance { get; set; }
        public ICommand BLEMatching { private set; get; }
        public ICommand Transfer { get; }
        public ICommand OpenSettings { get; }
        public ICommand ToggleAdapterState { get; }
        public ICommand SelectDevice { get; }
        public ICommand ShowAccount { get; }
        public ObservableCollection<ScanResultViewModel> Devices { get; }
    }
}
