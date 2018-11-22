using Acr.UserDialogs;
using Org.BouncyCastle.Security;
using Plugin.BluetoothLE;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing;
using ZXing.Net.Mobile.Forms;
using ZXing.QrCode;

namespace Hitcon2018.Wallet
{
    public class TransactionViewModel : ViewModel
    {
        private string address;
        public string Address {
            get => this.address;
            set => this.RaiseAndSetIfChanged(ref this.address, value);
        }
        private string balance;
        public string Balance {
            get => this.balance;
            set => this.RaiseAndSetIfChanged(ref this.balance, value);
        }

        private bool badgeEnable;
        public bool BadgeEnable
        {
            get => badgeEnable;
            set {
                this.RaiseAndSetIfChanged(ref this.badgeEnable, value);
                if (badgeEnable)
                {
                    this.cancellation.Cancel();
                    this.CanTransfer = true;
                }
            }
        }

        private bool canTransfer;
        public bool CanTransfer
        {
            get => this.canTransfer;
            set => this.RaiseAndSetIfChanged(ref this.canTransfer, value);
        }

        private bool isRefreshing = false;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => this.RaiseAndSetIfChanged(ref isRefreshing, value);
        }

        public CancellationTokenSource cancellation = new CancellationTokenSource();

        private ObservableCollection<TransactionHistory> transactionHistories;
        public ObservableCollection<TransactionHistory> TransactionHistories {
            get => this.transactionHistories;
            set => this.RaiseAndSetIfChanged(ref this.transactionHistories, value);
        } 
        public ICommand LoadTransactionHistory { get; }
        public ICommand SendTransaction { get; }
        public ICommand Recieve { get; }
        private string _address;
        private AccountSetting Account { get; set; }
        public TransactionViewModel(AccountSetting account)
        {
            Account = account;
            Address = Account.Address;
            TransactionHistories = new ObservableCollection<TransactionHistory>();
            Balance = Account.Amount;

            LoadHistory();
            if (Account.isBadge)
                UpdateBadgeStatus();
            else
                CanTransfer = true;

            this.LoadTransactionHistory = ReactiveCommand.Create(async () => {
                IsRefreshing = true;
                await LoadHistory();
                IsRefreshing = false;
            });

            this.SendTransaction = ReactiveCommand.Create(async () =>  {
                await App.Current.MainPage.Navigation.PushAsync(new TransferPageDetail
                {
                    BindingContext = new TransferPageViewModel()
                });
            });

            this.Recieve = ReactiveCommand.Create(async () => {
                await App.Current.MainPage.Navigation.PushAsync(new ContentPage
                {
                    Content = GenerateQR(address)
                });
            });
        }

        private void UpdateBadgeStatus()
        {
            CancellationTokenSource cts = this.cancellation;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (cts.IsCancellationRequested) return false;
                Task.Run(() =>
                {
                    this.BadgeEnable = Badge.Instance.CharacteristicTransaction != null;
                    //UserDialogs.Instance.Alert(Badge.Instance.CharacteristicTransaction.Uuid.ToString());
                });
                return true;
            });
        }

        ZXingBarcodeImageView GenerateQR(string codeValue)
        {
            var qrCode = new ZXingBarcodeImageView
            {
                BarcodeFormat = BarcodeFormat.QR_CODE,
                BarcodeOptions = new QrCodeEncodingOptions
                {
                    Height = 350,
                    Width = 350
                },
                BarcodeValue = codeValue,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            // Workaround for iOS
            qrCode.WidthRequest = 350;
            qrCode.HeightRequest = 350;
            return qrCode;
        }

        private async Task<bool> LoadHistory()
        {
            Balance = ((await EthService.Instance.GetTokenBalance(Account.Address, EthService.Instance.HitConCoinContract))/1000000000000000000).ToString();
            var result = await EthService.Instance.GetTransactionsAsync(Address);
            var descResult = result.OrderByDescending(item=>item.TransactionDate);
            TransactionHistories = new ObservableCollection<TransactionHistory>();
            foreach (var item in descResult)
            {
                //if (!TransactionHistories.Any(history => history.Hash == item.Hash))
                    TransactionHistories.Add(item);
            }
            return true;
        }
    }
}
