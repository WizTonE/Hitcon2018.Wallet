using Acr.UserDialogs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Hitcon2018.Wallet
{
    public class ScanQRCcodeViewModel : ViewModel
    {
        public ICommand ScanToggle;
        public ObservableCollection<QRcodeParingViewModel> PairingServices { get; }

        public ScanQRCcodeViewModel()
        {
            this.PairingServices = new ObservableCollection<QRcodeParingViewModel>();
            this.ScanToggle = ReactiveCommand.CreateFromTask(async () => 
            {
                var scanPage = new ZXingScannerPage();
                scanPage.Title = "請掃描條碼";
                scanPage.OnScanResult += (result) => {
                    // 停止掃描
                    scanPage.IsScanning = false;

                    // 顯示掃描結果
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.Navigation.PopAsync();
                        
                        //App.Current.MainPage.DisplayAlert("Scanned Barcode", result.Text, "OK");
                        //UserDialogs.Instance.Alert("Scanned Barcode", result.Text, "OK");
                        Pair(result.Text);
                        if(PairingServices.Count > 0)
                        {
                            var pairingObject = PairingServices[0];
                            App.Database.BadgeDAO.SaveItemAsync(new BadgeSetting { AESKey = pairingObject.AESkey });
                        }
                    });
                };

                // 導航到掃描條碼頁面
                await App.Current.MainPage.Navigation.PushAsync(scanPage);
            });
        }

        public void Pair(string pairingString)
        {
            var paringPattern = pairingString.Split('&');
            var paringObject = new QRcodeParingViewModel();
            foreach (var item in paringPattern)
            {
                switch (item.Substring(0,2))
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
                    default:
                        break;
                }
            }
            PairingServices.Add(paringObject);
        }
    }
}
