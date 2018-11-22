using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace Hitcon2018.Wallet
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ScanQRcodePage : ContentPage
	{
        public ObservableCollection<QRcodeParingViewModel> PairingServices { get; }
        public ScanQRcodePage()
		{
			InitializeComponent ();
            this.PairingServices = new ObservableCollection<QRcodeParingViewModel>();
        }
        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            // 產生掃描條碼頁面物件
            var scanPage = new ZXingScannerPage();
            scanPage.Title = "請掃描條碼";
            scanPage.OnScanResult += (result) => {
                // 停止掃描
                scanPage.IsScanning = false;

                // 顯示掃描結果
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    //App.Database.SaveItemAsync(new WalletSetting { ID = 0, AESKey = result.Text });
                    //DisplayAlert("Scanned Barcode", result.Text, "OK");
                    Pair(result.Text);
                    if (PairingServices.Count > 0)
                    {
                        var pairingObject = PairingServices[0];
                        App.Database.BadgeDAO.SaveItemAsync(new BadgeSetting { AESKey = pairingObject.AESkey,Address = pairingObject.Address, Characteristic = pairingObject.Characteristics, ServiceUUID = pairingObject.ServiceUUID  });
                        var abc = App.Database.BadgeDAO.GetItemsAsync().Result;
                    }
                });
            };

            // 導航到掃描條碼頁面
            await Navigation.PushAsync(scanPage);
        }

        public void Pair(string pairingString)
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
                    default:
                        break;
                }
            }
            PairingServices.Add(paringObject);
        }
    }
}