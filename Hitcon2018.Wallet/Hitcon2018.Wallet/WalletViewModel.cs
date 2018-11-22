using Acr.UserDialogs;
using Plugin.BluetoothLE;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Hitcon2018.Wallet
{
    public class WalletViewModel : ViewModel
    {
        private ObservableCollection<AccountSetting> _accounts = new ObservableCollection<AccountSetting>();
        public ObservableCollection<AccountSetting> Accounts
        {
            get => _accounts;
            set => this.RaiseAndSetIfChanged(ref _accounts, value);
        }
        public ObservableCollection<QRcodeParingViewModel> PairingServices { get; } = new ObservableCollection<QRcodeParingViewModel>();
        public ICommand AddWallet { get; }
        public ICommand LoadAccounts { get; }
        public ICommand SelectedAccount { get; }
        public ICommand Reset { get; }
        public ICommand Disconnect { get; }
        public ICommand Setting { get; }

        private bool _badgeEnable;
        public bool BadgeEnable
        {
            get => _badgeEnable;
            set => this.RaiseAndSetIfChanged(ref _badgeEnable, value);
        }

        private string PrivateKey { get; set; }
        public WalletViewModel()
        {
            var reloadAccounts = ReloadAccounts();
            UpdateAccountBalance();
            if (CrossBleAdapter.Current.CanControlAdapterState())
            {
                CrossBleAdapter.Current.SetAdapterState(false);
            }

            AddWallet = ReactiveCommand.Create(async () =>
            {
#if __ANDROID__
	                // Initialize the scanner first so it can track the current context
	                MobileBarcodeScanner.Initialize (Application);
#endif
                var scanPage = new ZXingScannerPage();
                scanPage.Title = "請掃描條碼";
                scanPage.OnScanResult += (result) =>
                {
                    // 停止掃描
                    scanPage.IsScanning = false;

                    // 顯示掃描結果
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                        var userInput = await UserDialogs.Instance.PromptAsync(new PromptConfig { OkText = "Import", CancelText = "Cancel", Title = "Enter your Acount Name", InputType = InputType.Name });
                        if (!userInput.Ok) return;
                        var accountName = userInput.Text;
                        var addressText = result.Text;
                        var addressSplitArray = addressText.Split('?');
                        var isBadgeAddress = addressSplitArray.Length > 1;
                        if (isBadgeAddress)
                        {
                            await Pair(addressText, accountName);
                                //this.PrivateKey = addressSplitArray.Where(item => item.Substring(0, 2) == "a=").Select(item => item.Substring(2)).First();
                            }
                        else
                        {
                            PrivateKey = addressText;
                                //addressSplitArray.Where(item => item.Substring(0, 2) == "a=").Select(item => item.Substring(2)).First()
                                //: addressText;
                                var address = EthService.Instance.GetAddressByPrivateKey(PrivateKey);
                            var balance = await EthService.Instance.GetTokenBalance(address, EthService.Instance.HitConCoinContract);
                            var isAccountExist = await App.Database.AccountDAO.GetItemAsync(address) != null;
                            if (!isAccountExist)
                                await App.Database.AccountDAO.InsertItemAsync(new AccountSetting { Address = address, AccountName = accountName, PrivateKey = PrivateKey, Amount = (balance / 1000000000000000000).ToString(), Type = 0 });
                            else
                                UserDialogs.Instance.Alert("Account existed...");
                            await ReloadAccounts();
                        }
                    });
                };

                // 導航到掃描條碼頁面
                await Application.Current.MainPage.Navigation.PushAsync(scanPage);
            });

            LoadAccounts = ReactiveCommand.Create(async () =>
            {
                await ReloadAccounts();
            });

            SelectedAccount = ReactiveCommand.CreateFromTask<AccountSetting>(async item =>
            {
                if (item.Type == 1 && (Badge.Instance.BleDevice == null || Badge.Instance.BleDevice.Status != ConnectionStatus.Connected))
                {
                    if (CrossBleAdapter.Current.CanControlAdapterState())
                    {
                        var adapter = CrossBleAdapter.Current;
                        if(adapter.Status  == AdapterStatus.PoweredOff)
                            CrossBleAdapter.Current.SetAdapterState(true);
                        await Task.Delay(3000);
                    }
                    await ScanBadge();
                }

                //var tokenNumberString = amount.Substring(0, minimalLength) + "." + amount.Substring(minimalLength);
                AccountService.Instance.AccountSetting = item;
                await Application.Current.MainPage.Navigation.PushAsync(new TransactionPage
                {
                    BindingContext = new TransactionViewModel(item)
                });
            });

            Reset = ReactiveCommand.Create(async () =>
            {
                await App.Database.AccountDAO.DeleteItemsAync();
                await App.Database.BadgeDAO.DeleteItemsAync();
                await App.Database.TransactionDAO.DeleteItemsAync();
                App.Database.ReBuildDataBase();
            });

            Disconnect = ReactiveCommand.Create(() =>
            {
                Badge.Instance.Disconnect();
            });

            Setting = ReactiveCommand.Create(async () =>
            {
                var command = await UserDialogs.Instance.ActionSheetAsync("Settings", "Back", "", null, new string[] { "Toggle BLE Adapter", "Disconnect to badge", "Reset All" });
                switch (command)
                {
                    case "Toggle BLE Adapter":
                        if (CrossBleAdapter.Current.CanControlAdapterState())
                        {
                            var adapter = CrossBleAdapter.Current;
                            if (adapter.Status == AdapterStatus.PoweredOff)
                                CrossBleAdapter.Current.SetAdapterState(true);
                            if (adapter.Status == AdapterStatus.PoweredOn)
                                CrossBleAdapter.Current.SetAdapterState(false);
                        }
                        break;
                    case "Disconnect to badge":
                        if (await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig { Title = "Confirm?", OkText = "Ok", CancelText = "Back" }))
                        {
                            Badge.Instance.Disconnect();
                            Badge.Instance.cancellation.Cancel();
                        }
                        break;
                    case "Reset All":
                        if (await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig { Title = "Reset All?", OkText = "Ok", CancelText = "Back" }))
                        {
                            await App.Database.AccountDAO.DeleteItemsAync();
                            await App.Database.BadgeDAO.DeleteItemsAync();
                            await App.Database.TransactionDAO.DeleteItemsAync();
                            Accounts.Clear();
                            App.Database.ReBuildDataBase();
                            Badge.Instance.ResetAll();
                        }
                        break;
                }
            });
        }

        private void UpdateAccountBalance()
        {
            Device.StartTimer(TimeSpan.FromSeconds(30), () =>
            {
                Task.Run(async () =>
                {
                    var tempAccounts = new ObservableCollection<AccountSetting>();
                    foreach (var account in Accounts)
                    {
                        account.Amount = (await EthService.Instance.GetTokenBalance(account.Address, EthService.Instance.HitConCoinContract) / 1000000000000000000).ToString();
                        await App.Database.AccountDAO.UpdateItemAsync(account);
                        tempAccounts.Add(account);
                    }
                    Accounts = null;
                    Accounts = tempAccounts;
                });
                return true;
            });
        }

        private async Task<bool> ReloadAccounts()
        {
            foreach (var account in await App.Database.AccountDAO.GetItemsAsync())
            {
                if (Accounts.All(item => item.Address != account.Address))
                    Accounts.Add(account);
            }
            return true;
        }

        private async Task<bool> Pair(string pairingString, string accountName)
        {
            var paringPattern = pairingString.Split('&');
            var paringObject = new QRcodeParingViewModel();
            foreach (var item in paringPattern)
            {
                switch (item.Substring(0, 2))
                {
                    case "a=":
                        paringObject.Address = item.Substring(2);
                        break;
                    case "k=":
                        paringObject.AESkey = item.Substring(2);
                        break;
                    case "s=":
                        paringObject.ServiceUUID = item.Substring(2);
                        break;
                    case "c=":
                        paringObject.Characteristics = item.Substring(2);
                        break;
                }
            }
            PairingServices.Add(paringObject);
            if (PairingServices.Count > 0)
            {
                var pairingObject = PairingServices[0];
                var badgeSetting = new BadgeSetting { AESKey = pairingObject.AESkey, Address = pairingObject.Address, Characteristic = pairingObject.Characteristics, ServiceUUID = pairingObject.ServiceUUID };
                await App.Database.BadgeDAO.SaveItemAsync(badgeSetting);
                //var abc = App.Database.BadgeDAO.GetItemsAsync().Result;
                var balance = await EthService.Instance.GetTokenBalance("0x" + badgeSetting.Address, EthService.Instance.HitConCoinContract);
                var isAccountExist = await App.Database.AccountDAO.GetItemAsync(badgeSetting.Address) != null;
                if (!isAccountExist)
                    await App.Database.AccountDAO.InsertItemAsync(new AccountSetting { Address = "0x" + badgeSetting.Address, AccountName = accountName, PrivateKey = PrivateKey, Amount = (balance / 1000000000000000000).ToString(), Type = 1 });
                else
                    UserDialogs.Instance.Alert("Account existed...");
                await ReloadAccounts();
                //await ScanBadge();
            }
            return true;
        }

        private async Task<bool> ScanBadge()
        {
            var walletSettings = await App.Database.BadgeDAO.GetItemsAsync();
            var walletSetting = walletSettings.Last();
            var badgeSeviceUuid = walletSetting != null ? new Guid(walletSetting.ServiceUUID) : default(Guid);
            var scan = default(IDisposable);
            var findBadge = default(IScanResult);
            scan = CrossBleAdapter.Current.Scan().Buffer(TimeSpan.FromSeconds(0.5)).Subscribe(async results =>
            {

                foreach (var item in results)
                {
                    try
                    {
                        if (item.AdvertisementData.ServiceUuids.Length <= 0 ||
                            !item.AdvertisementData.ServiceUuids[0].Equals(badgeSeviceUuid)) continue;
                        findBadge = item;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                if (findBadge == null) return;
                //findBadge = new ScanResult(new Device)
                scan?.Dispose();
                //App.Current.MainPage.Navigation.PushAsync(new DevicePage
                //{
                //    BindingContext = new DeviceViewModel(findBadge.Device)
                //});
                //Badge.Instance.ConnectToBadge();
                await ConnectToBadge(findBadge.Device);

            });

            return true;
        }

        private async Task<bool> ConnectToBadge(IDevice device)
        {
            Badge.Instance.BleDevice = device;
            await Badge.Instance.ConnectToBadge();
            Badge.Instance.UpdateBalanceTask();
            return true;
        }
    }
}
