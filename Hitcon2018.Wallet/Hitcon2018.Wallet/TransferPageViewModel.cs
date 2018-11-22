using Acr.UserDialogs;
using Org.BouncyCastle.Security;
using Plugin.BluetoothLE;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Hitcon2018.Wallet
{
    public class TransferPageViewModel : ViewModel
    {
        private DateTime _lastValue;
        //private IGattCharacteristic CharacteristicTransaction;
        //private IGattCharacteristic CharacteristicTXN;
        //private IGattCharacteristic CharacteristicUpdateBalance;
        //private IGattCharacteristic CharacteristicAddERC20;

        string _toAddress;
        public string ToAddress
        {
            get => _toAddress;
            set => this.RaiseAndSetIfChanged(ref _toAddress, value);
        }

        
        string _amount;
        public string Amount
        {
            get => _amount;
            set => this.RaiseAndSetIfChanged(ref _amount, value);
        }

        string _gasprice;
        public string Gasprice
        {
            get => _gasprice;
            set => this.RaiseAndSetIfChanged(ref _gasprice, value);
        }

        string _gaslimit;
        public string Gaslimit
        {
            get => _gaslimit;
            set => this.RaiseAndSetIfChanged(ref _gaslimit, value);
        }

        public string TxN { get; set; }

        public ICommand Transfer { get; }
        public ICommand Connect { get; }
        public ICommand ScanAddress { get; }
        public ICommand UpdateBalance { get; }

        public int Type { get; set; }
        private readonly string FromAddress;
        private string TransAmount { get; set; }
        public TransferPageViewModel()
        {
            Gasprice = "99000000000";
            Gaslimit = "300000";

            var hasAccountSetting = AccountService.Instance.AccountSetting != null;
            if (hasAccountSetting)
            {
                FromAddress = AccountService.Instance.AccountSetting?.Address;
                Type = AccountService.Instance.AccountSetting.Type;
            }
            else
            {
                var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                FromAddress = badgeSetting.Address;
                Type = 1;
            }

            //Connect = ReactiveCommand.Create(async () =>
            //{
            //    //using (var loader = UserDialogs.Instance.Loading("Connecting to badge...",null,"Cancel",true, MaskType.Black))
            //    //{
            //    //    try
            //    //    {
            //    //        loader.Title = "Pairing...";
            //    //        if (!await Badge.Instance.PairBLE())
            //    //            throw new Exception("Paring failed");
            //    //        await Task.Delay(2000);
            //    //        loader.Title = "Connecting...";
            //    //        Badge.Instance.Connect();
            //    //        await Task.Delay(2000);
            //    //        loader.Title = "MTU Changing...";
            //    //        var actualMTU = await Badge.Instance.RequestMTU();
            //    //        await Task.Delay(2000);
            //    //        loader.Title = "Function Binding...";
            //    //        //var charactertiscs = await Badge.Instance.GetGattCharacteristics();
            //    //        var charactertiscs = await Badge.Instance.GetCharacteristics();
            //    //        CharacteristicTransaction = charactertiscs[Badge.BadgeFunction.Transaction];
            //    //        CharacteristicTXN = charactertiscs[Badge.BadgeFunction.TXN];
            //    //        CharacteristicUpdateBalance = charactertiscs[Badge.BadgeFunction.Balance];
            //    //        loader.Title = "Notifying...";
            //    //        await Badge.Instance.ToggleNotify(CharacteristicTXN);
            //    //        //var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
            //    //        //var balance = await EthService.Instance.GetTokenBalance(badgeSetting.Address, EthService.Instance.HitConCoinContract);
            //    //        //var discard = 0;
            //    //        //await Badge.Instance.DoWrite(CharacteristicUpdateBalance, true, HexEncoding.GetBytes(EthService.Instance.HitConCoinContract + String.Format("0{0:X}", balance), out discard));
            //    //    }
            //    //    catch(Exception ex)
            //    //    {
            //    //        UserDialogs.Instance.Alert(ex.ToString());
            //    //    }
            //    //}

            //    //UserDialogs.Instance.Toast("Badge connected...", new TimeSpan(1000));
            //});
            

            Transfer = ReactiveCommand.Create(async () =>
            {
                TransAmount = Amount;
                if (TransAmount.Length < 18)
                    TransAmount += "000000000000000000";
                if (Type == 0)
                {
                    await EthService.Instance.Transfer(AccountService.Instance.AccountSetting.PrivateKey, FromAddress, _toAddress, BigInteger.Parse(TransAmount), BigInteger.Parse(Gaslimit), BigInteger.Parse(Gasprice));
                    UserDialogs.Instance.Alert("Transaction Finished");
                }
                else
                {
                    await BadgeTransaction();
                    UserDialogs.Instance.Alert("Please confirm the transaction on the Badge");
                }

                await Application.Current.MainPage.Navigation.PopAsync();
            });

            ScanAddress = ReactiveCommand.Create(async () => {
                var scanPage = new ZXingScannerPage();
                scanPage.Title = "請掃描條碼";
                scanPage.OnScanResult += (result) => {
                    // 停止掃描
                    scanPage.IsScanning = false;

                    // 顯示掃描結果
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Application.Current.MainPage.Navigation.PopAsync();
                        //this.ToAddress = result.Text.Split('?').Where(item => item.Substring(0, 2) == "a=").Select(item => item.Substring(2)).First();
                        ToAddress = result.Text;
                    });
                };

                // 導航到掃描條碼頁面
                await Application.Current.MainPage.Navigation.PushAsync(scanPage);
            });

            UpdateBalance = ReactiveCommand.Create( async() => {
                var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                var aesKey = badgeSetting.AESKey;
                var balance = (double)(await EthService.Instance.GetTokenBalance("0x"+badgeSetting.Address, EthService.Instance.HitConCoinContract))/1000000000000000000;

                var hexOfContractBalance = BitConverter.ToString(BitConverter.GetBytes((double)(await EthService.Instance.GetTokenBalance("0x" + badgeSetting.Address, EthService.Instance.HitConCoinContract)) / 1000000000000000000)).Replace("-", string.Empty);
                var hexOfBalance = BitConverter.ToString(BitConverter.GetBytes(balance)).Replace("-",string.Empty);
                //var contractAddress = EthService.Instance.HitConCoinContract.Remove(0, 2);//string.Format("{0:X2}", balance.ToString("X2").Length / 2) + balance.ToString("X2")
                //var contractAddress = "503b0C139665E7e9F863ba1BCf0e635a2E87aA5b";
                var contractAddress = EthService.Instance.HitConCoinContract.Remove(0, 2);
                var plainData = HexEncoding.GetBytes("01" + $"{contractAddress.Length / 2:X2}" + contractAddress +
                                "02" + "08" + hexOfContractBalance +
                                "01" + $"{"da7da98169dbee184e77ed44f6da1536a379c8d4".Length / 2:X2}" + "da7da98169dbee184e77ed44f6da1536a379c8d4" +
                                "02" + "08" + hexOfBalance, out _);
                byte[] encrypteddata = Encryption(plainData, aesKey);
                await Badge.Instance.DoWrite(Badge.Instance.CharacteristicUpdateBalance, true, encrypteddata);
            });
        }

        private async Task<bool> BadgeTransaction()
        {
            var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
            var aesKey = badgeSetting.AESKey;
            var htoaddress = _toAddress.Replace("0x",string.Empty);
            var htoContractAddress = EthService.Instance.HitConCoinContract.Substring(2);
            //amount = 1000000000000000000;
            //gasprice = 99000000000;
            //gaslimit = 300000;
            var hvalue = BigInteger.Parse(TransAmount).ToString("X2");
            var hgasprice = BigInteger.Parse(_gasprice).ToString("X2");
            if (hgasprice.Length % 2 == 1)
                hgasprice = "0" + hgasprice;
            //var hgaslimit = String.Format("0{0:X}", gaslimit);
            var hgaslimit = BigInteger.Parse(_gaslimit).ToString("X2");
            if (hgaslimit.Length % 2 == 1)
                hgaslimit = "0" + hgaslimit;
            var noice = await EthService.Instance.GetNonceByAddress(badgeSetting.Address);
            var hnoice = (noice).ToString("X2");
            var hdata = "a9059cbb" + htoaddress.Replace("0x",string.Empty).PadLeft(64,'0') + hvalue.PadLeft(64,'0');
            //var hdata = "";
            var transactionArray =
                "01" + $"{htoContractAddress.Length / 2:X2}" + htoContractAddress +
                        "02" + $"{"00".Length / 2:X2}" + "00" +
                        "03" + $"{hgasprice.Length / 2:X2}" + hgasprice +
                        "04" + $"{hgaslimit.Length / 2:X2}" + hgaslimit +
                        "05" + $"{hnoice.Length / 2:X2}" + hnoice +
                        "06" + $"{hdata.Length / 2:X2}" + hdata;
            transactionArray = transactionArray.PadRight(128, '0');

            var plainData = HexEncoding.GetBytes(transactionArray, out _);

            var encrypteddata = Encryption(plainData, aesKey);
            await Badge.Instance.DoWrite(Badge.Instance.CharacteristicTransaction, true, encrypteddata);
            Badge.Instance.CharacteristicTXN.RegisterAndNotify()
                .Subscribe(x => this.SetTxNValue(x, false));
            return true;
        }

        private byte[] Encryption(byte[] data, string aesKey)
        {
            byte[] iv = new byte[16];
            SecureRandom random = new SecureRandom();
            random.NextBytes(iv);
            var key = HexEncoding.GetBytes(aesKey, out _);
            return HexEncoding.GetBytes(BitConverter.ToString(iv) + BitConverter.ToString(EncryptAES(iv, key, data)), out _);
        }

        void SetTxNValue(CharacteristicGattResult result, bool fromUtf8) => Device.BeginInvokeOnMainThread(async () =>
        {
            _lastValue = DateTime.Now;

            if (result.Data == null)
                TxN = "EMPTY";

            else
                TxN = fromUtf8
                    ? Encoding.UTF8.GetString(result.Data, 0, result.Data.Length)
                    : BitConverter.ToString(result.Data);
            var transactionData = "0x" + TxN.Replace("-", string.Empty);
            await EthService.Instance.SendTransaction(transactionData);
            //if (res == null) UserDialogs.Instance.Alert("Transaction canceled");
            //UserDialogs.Instance.Alert(Convert.ToString(TxN));
        });

        private byte[] EncryptAES(byte[] iv, byte[] key, byte[] text)
        {
            try 
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider
                {
                    Key = key,
                    IV = iv
                };
                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(text, 0, text.Length);
                    cs.FlushFinalBlock();
                    Convert.ToBase64String(ms.ToArray());
                }


                return ms.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
