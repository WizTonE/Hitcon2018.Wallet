using Acr.UserDialogs;
using Plugin.BluetoothLE;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Threading.Tasks;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace Hitcon2018.Wallet
{
    public class Badge
    {
        public enum BadgeFunction
        {
            Transaction,
            TXN,
            AddERC20,
            Balance,
            General_Purpose_Cmd,
            General_Purpose_Data
        }

        private static Badge instance;
        IDisposable watcher;
        private string Value;
        private bool IsValueAvailable;
        private DateTime LastValue;
        private bool IsNotifying;

        public CancellationTokenSource cancellation = new CancellationTokenSource();
        public IGattCharacteristic CharacteristicTransaction { get; set; }
        public IGattCharacteristic CharacteristicTXN { get; set; }
        public IGattCharacteristic CharacteristicUpdateBalance { get; set; }
        public IGattCharacteristic CharacteristicAddERC20 { get; set; }
        public IGattCharacteristic CharacteristicGeneralCmd { get; set; }
        public IGattCharacteristic CharacteristicGeneralData { get; set; }

        public static Badge Instance { get { return instance ?? (instance = new Badge()); } }
        public IDevice BleDevice { get; set; }
        public ICommand RequestMtu { get; }
        public ObservableCollection<Group<IGattCharacteristic>> GattCharacteristics { get; } = new ObservableCollection<Group<IGattCharacteristic>>();

        public Dictionary<BadgeFunction, IGattCharacteristic> BadgeFunctionDicts { get; } = new Dictionary<BadgeFunction, IGattCharacteristic>();
        private string[] FunctionMapping = new string[] { "Transaction", "TXN", "AddERC20", "Balance", "General Purpose Cmd", "General Purpose Data" };

        private Badge()
        {

        }

        public async Task<bool> ConnectToBadge()
        {
            //using (var loader = UserDialogs.Instance.Loading("Connecting to badge...", null, "Cancel", true, MaskType.Black))
            //{
            try
            {
                UserDialogs.Instance.Toast("Connecting to Badge.. Please wait", new TimeSpan(1000));
                //loader.Title = "Connecting...";
                Connect();

                //await Task.Delay(2000);
                //loader.Title = "Pairing...";
                //var status = await PairBLE();
                
                
                //loader.Title = "MTU Changing...";
                var actualMTU = await Badge.Instance.RequestMTU();
                //await Task.Delay(5000);
                //UserDialogs.Instance.Toast("Waiting for the service...", new TimeSpan(2000));
                var walletSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                var walletChr = walletSetting.Characteristic;
                var chrOffsetString = walletSetting.ServiceUUID.Substring(8);
                var charrr = Enumerable.Range(0, walletChr.Length / 8).Select(i => new Guid(walletChr.Substring(i * 8, 8) + chrOffsetString)).ToArray();
                BleDevice.WhenAnyCharacteristicDiscovered().Subscribe(async chs => {
                    if (chs.Uuid == charrr[0]) CharacteristicTransaction = chs;
                    if (chs.Uuid == charrr[1]) { CharacteristicTXN = chs; CharacteristicTXN.EnableNotifications(); }
                    if (chs.Uuid == charrr[2]) CharacteristicAddERC20 = chs;
                    if (chs.Uuid == charrr[3]) CharacteristicUpdateBalance = chs;
                    if (chs.Uuid == charrr[4]) CharacteristicGeneralCmd = chs;
                    if (chs.Uuid == charrr[5]) CharacteristicGeneralData = chs;
                    if (CharacteristicTransaction != null) UserDialogs.Instance.Toast("Badge connected...", new TimeSpan(1000));
                });

                //GetCharacteristics();
                //var walletSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                //var walletChr = walletSetting.Characteristic;
                //var chrOffsetString = walletSetting.ServiceUUID.Substring(8);
                //var charrr = Enumerable.Range(0, walletChr.Length / 8).Select(i => new Guid(walletChr.Substring(i * 8, 8) + chrOffsetString)).ToArray();

                //foreach (var item in charrr)
                //{
                //    var abc = await BleDevice.GetKnownService(new Guid(walletSetting.ServiceUUID));
                //    var chs = await abc.GetKnownCharacteristics(item);
                //    //var chs =  await BleDevice.GetKnownCharacteristics(new Guid(walletSetting.ServiceUUID), item);
                //    if (chs.Uuid == charrr[0]) CharacteristicTransaction = chs;
                //    if (chs.Uuid == charrr[1]) { CharacteristicTXN = chs; CharacteristicTXN.EnableNotifications(); }
                //    if (chs.Uuid == charrr[2]) CharacteristicAddERC20 = chs;
                //    if (chs.Uuid == charrr[3]) CharacteristicUpdateBalance = chs;
                //    if (chs.Uuid == charrr[4]) CharacteristicGeneralCmd = chs;
                //    if (chs.Uuid == charrr[5]) CharacteristicGeneralData = chs;
                //}
                //await Task.Delay(10000);
                //loader.Title = "Function Binding...";
                //var charactertiscs = await Badge.Instance.GetGattCharacteristics();
                //CharacteristicTransaction = charactertiscs[Badge.BadgeFunction.Transaction];
                //CharacteristicTXN = charactertiscs[Badge.BadgeFunction.TXN];
                //CharacteristicUpdateBalance = charactertiscs[Badge.BadgeFunction.Balance];
                //loader.Title = "Notifying...";
                //await Badge.Instance.ToggleNotify(CharacteristicTXN);
                //var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                //var balance = await EthService.Instance.GetTokenBalance(badgeSetting.Address, EthService.Instance.HitConCoinContract);
                //var discard = 0;
                //await Badge.Instance.DoWrite(CharacteristicUpdateBalance, true, HexEncoding.GetBytes(EthService.Instance.HitConCoinContract + String.Format("0{0:X}", balance), out discard));

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.ToString());
            }
            //}

            
            return true;
        }

        public async Task<bool> PairBLE()
        {
            var result = false;
            if (this.BleDevice.PairingStatus != PairingStatus.Paired)
                this.BleDevice.PairingRequest().Subscribe(isSuccuss => { });
                //result = await this.BleDevice.PairingRequest();
            return result;
        }

        public void Connect()
        {
            if (this.BleDevice.Status != ConnectionStatus.Disconnected)
                return;
            this.BleDevice.Connect(new ConnectionConfig { AutoConnect = true });
            this.BleDevice.ConnectWait();
            //}
            //else
            //{
            //    this.BleDevice.CancelConnection();
            //}
        }

        public async Task<int> RequestMTU()
        {
            var actual = 0;
            if (!this.BleDevice.Features.HasFlag(DeviceFeatures.MtuRequests))
            {
                UserDialogs.Instance.Alert("MTU Request not supported on this platform");
            }
            else
            {
                actual = await this.BleDevice.RequestMtu(512);
                //UserDialogs.Instance.Toast("MTU Changed to " + actual);
            }
            return actual;
        }

        public async Task<Dictionary<BadgeFunction, IGattCharacteristic>> GetGattCharacteristics()
        {
            var walletSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();

            this.BleDevice
                .WhenAnyCharacteristicDiscovered()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(chs =>
               {
                   var serviceUuid = chs.Service.Uuid.ToString();
                   if (serviceUuid == walletSetting.ServiceUUID)
                   {
                       try
                       {

                           var service = this.GattCharacteristics.FirstOrDefault(x => x.ShortName.Equals(chs.Service.Uuid.ToString()));
                           if (service == null)
                           {
                               service = new Group<IGattCharacteristic>(
                                   $"{chs.Service.Description} ({chs.Service.Uuid})",
                                   chs.Service.Uuid.ToString()
                               );
                               this.GattCharacteristics.Add(service);
                           }

                           service.Add(chs);
                       }

                       catch (Exception ex)
                       {
                           // eat it
                           Console.WriteLine(ex);
                       }
                   }
               });
            //var Characteristics = await this.BleDevice.DiscoverServices();
            await Task.Delay(10000);

            var index = 0;
            foreach (var gatt in GattCharacteristics[0])
            {
                BadgeFunctionDicts.Add((BadgeFunction)index, gatt);
                index++;
            }
            return BadgeFunctionDicts;
        }

        public void ResetAll()
        {
            BleDevice.CancelConnection();
            BleDevice = null;
            CharacteristicAddERC20 = null;
            CharacteristicTransaction = null;
            CharacteristicTXN = null;
            CharacteristicUpdateBalance = null;
            CharacteristicGeneralCmd = null;
            CharacteristicGeneralData = null;
        }

        //public async Task<Dictionary<BadgeFunction, IGattCharacteristic>> GetCharacteristics()
        public void GetCharacteristics()
        {
            var walletSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
            var walletChr = walletSetting.Characteristic;
            var chrOffsetString = walletSetting.ServiceUUID.Substring(8);
            var charrr = Enumerable.Range(0, walletChr.Length / 8).Select(i => new Guid(walletChr.Substring(i * 8, 8) + chrOffsetString)).ToArray();
            BleDevice.WhenAnyCharacteristicDiscovered().Subscribe(async chs => {
                if (chs.Uuid == charrr[0]) CharacteristicTransaction = chs;
                if (chs.Uuid == charrr[1]) { CharacteristicTXN = chs; CharacteristicTXN.EnableNotifications(); }
                if (chs.Uuid == charrr[2]) CharacteristicAddERC20 = chs;
                if (chs.Uuid == charrr[3]) CharacteristicUpdateBalance = chs;
                if (chs.Uuid == charrr[4]) CharacteristicGeneralCmd = chs;
                if (chs.Uuid == charrr[5]) CharacteristicGeneralData = chs;
                if (CharacteristicTransaction != null) UserDialogs.Instance.Toast("Badge connected...", new TimeSpan(1000));
            });
            //var allCharactertistics = BleDevice.GetKnownCharacteristics(new Guid(walletSetting.ServiceUUID), charrr)
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(
            //   chs =>
            //   {
            //        //if (chs.Uuid == charrr[0] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.Transaction)) BadgeFunctionDicts.Add(BadgeFunction.Transaction, chs);
            //        //if (chs.Uuid == charrr[1] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.TXN)) BadgeFunctionDicts.Add(BadgeFunction.TXN, chs);
            //        //if (chs.Uuid == charrr[2] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.AddERC20)) BadgeFunctionDicts.Add(BadgeFunction.AddERC20, chs);
            //        //if (chs.Uuid == charrr[3] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.Balance)) BadgeFunctionDicts.Add(BadgeFunction.Balance, chs);
            //        //if (chs.Uuid == charrr[4] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.General_Purpose_Cmd)) BadgeFunctionDicts.Add(BadgeFunction.General_Purpose_Cmd, chs);
            //        //if (chs.Uuid == charrr[5] && !BadgeFunctionDicts.ContainsKey(BadgeFunction.General_Purpose_Data)) BadgeFunctionDicts.Add(BadgeFunction.General_Purpose_Data, chs);
            //        if (chs.Uuid == charrr[0]) CharacteristicTransaction = chs;
            //       if (chs.Uuid == charrr[1]) { CharacteristicTXN = chs;  CharacteristicTXN.EnableNotifications(); }
            //       if (chs.Uuid == charrr[2]) CharacteristicAddERC20 = chs;
            //       if (chs.Uuid == charrr[3]) CharacteristicUpdateBalance = chs;
            //       if (chs.Uuid == charrr[4]) CharacteristicGeneralCmd = chs;
            //       if (chs.Uuid == charrr[5]) CharacteristicGeneralData = chs;
            //       if (CharacteristicTransaction != null) UserDialogs.Instance.Toast("Badge connected...", new TimeSpan(1000));
            //   });
            //await Task.Delay(10000);
            //CharacteristicTransaction = BadgeFunctionDicts[BadgeFunction.Transaction];
            //CharacteristicTXN = BadgeFunctionDicts[BadgeFunction.TXN];
            //CharacteristicAddERC20 = BadgeFunctionDicts[BadgeFunction.AddERC20];
            //CharacteristicUpdateBalance = BadgeFunctionDicts[BadgeFunction.Balance];
        }



        public async Task<byte[]> DoWrite(IGattCharacteristic Characteristic, bool withResponse, byte[] bytes)
        {

            //if (withResponse)
            //{
            //    Characteristic
            //        .Write(bytes)
            //        .Timeout(TimeSpan.FromSeconds(2))
            //        .Subscribe(
            //            x => UserDialogs.Instance.Toast("Write Complete"),
            //            ex => UserDialogs.Instance.Alert(ex.ToString())
            //        );
            //}
            //else
            //{
            //    Characteristic
            //        .WriteWithoutResponse(bytes)
            //        .Timeout(TimeSpan.FromSeconds(2))
            //        .Subscribe(
            //            x => UserDialogs.Instance.Toast("Write Without Response Complete"),
            //            ex => UserDialogs.Instance.Alert(ex.ToString())
            //        );
            //}

            if ( BleDevice.Status != ConnectionStatus.Connected)
            {
                await Reconnect();
            }
            if (Characteristic == null)
            {
                UserDialogs.Instance.Alert("Funtion failed, Please reconnect the Badge");
                return null;
            }
            var returnResult = await Characteristic.Write(bytes);
            //Characteristic.WhenNotificationReceived().Subscribe(item => {
            //    returnResult = item.Data;
            //});
            return returnResult.Data;
        }

        public async Task<CharacteristicGattResult> GetNotifyResult(IGattCharacteristic characteristic)
        {
            await characteristic.EnableNotifications();
            var TXN = await characteristic.RegisterAndNotify();
            return await characteristic.WhenNotificationReceived();
        }

        public async Task<bool> ToggleNotify(IGattCharacteristic Characteristic)
        {
            if (Characteristic.IsNotifying)
            {
                this.watcher?.Dispose();
                this.IsNotifying = false;
            }
            else
            {
                this.IsNotifying = true;
                //var utf8 = await UserDialogs.Instance.ConfirmAsync(
                //    "Display Value as UTF8 or HEX?",
                //    okText: "UTF8",
                //    cancelText: "HEX"
                //var utf8 = false;
                ////);
                //this.watcher = Characteristic
                //    .RegisterAndNotify()
                //    .Subscribe(x => this.SetReadValue(x, utf8));
                await Characteristic.EnableNotifications();
            }
            return IsNotifying;
        }

        public async Task<bool> Reconnect()
        {
            //await PairBLE();
            await Task.Delay(1000);
            Connect();
            await Task.Delay(1000);
            var actualMTU = await Badge.Instance.RequestMTU();
            await Task.Delay(2000);
            return true;
        }

        public void Disconnect()
        {
            this.BleDevice.CancelConnection();
        }

        public void UpdateBalanceTask()
        {
            CancellationTokenSource cts = this.cancellation;
            Device.StartTimer(TimeSpan.FromSeconds(30), () =>
            {
                if (cts.IsCancellationRequested) return false;
                if (BleDevice == null)
                    return true;
                if (BleDevice.Status != ConnectionStatus.Connected)
                {
                    //Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
                    UserDialogs.Instance.Toast("Can't find the badge near you..");
                    Task.Run(async () =>
                    {
                        await Reconnect();
                    });
                    return true;
                }
                Task.Run(async () =>
                {
                    var badgeSetting = App.Database.BadgeDAO.GetItemsAsync().Result.Last();
                    var aesKey = badgeSetting.AESKey;
                    var discard = 0;

                    var hexOfContractBalance = BitConverter.ToString(BitConverter.GetBytes((double)(await EthService.Instance.GetTokenBalance("0x" + badgeSetting.Address, EthService.Instance.HitConCoinContract)) / 1000000000000000000)).Replace("-", string.Empty);
                    var hexOfEthBalance = BitConverter.ToString(BitConverter.GetBytes((double)(await EthService.Instance.GetBalance("0x" + badgeSetting.Address)) / 1000000000000000000)).Replace("-", string.Empty);
                    //var contractAddress = EthService.Instance.HitConCoinContract.Remove(0, 2);//string.Format("{0:X2}", balance.ToString("X2").Length / 2) + balance.ToString("X2")
                    var contractAddress = EthService.Instance.HitConCoinContract.Remove(0, 2);
                    var plainData = HexEncoding.GetBytes("01" + string.Format("{0:X2}", contractAddress.Length / 2) + contractAddress +
                                    "02" + "08" + hexOfContractBalance +
                                    "01" + string.Format("{0:X2}", badgeSetting.Address.Length / 2) + badgeSetting.Address +
                                    "02" + "08" + hexOfEthBalance, out discard);
                    byte[] encrypteddata = Encryption(plainData, aesKey);
                    await Badge.Instance.DoWrite(Badge.Instance.CharacteristicUpdateBalance, true, encrypteddata);
                });
                return true;
            });
        }

        private byte[] Encryption(byte[] data, string aesKey)
        {
            var discarded = 0;
            byte[] iv = new byte[16];
            SecureRandom random = new SecureRandom();
            random.NextBytes(iv);
            var key = HexEncoding.GetBytes(aesKey, out discarded);
            return HexEncoding.GetBytes(BitConverter.ToString(iv) + BitConverter.ToString(EncryptAES(iv, key, data)), out discarded);
        }

        private byte[] EncryptAES(byte[] iv, byte[] key, byte[] text)
        {
            try
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider
                {
                    Key = key,
                    IV = iv
                };
                string encrypt = "";
                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(text, 0, text.Length);
                    cs.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(ms.ToArray());
                }


                return ms.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        void SetReadValue(CharacteristicGattResult result, bool fromUtf8) => Device.BeginInvokeOnMainThread(() =>
        {
            this.IsValueAvailable = true;
            this.LastValue = DateTime.Now;

            if (result.Data == null)
                this.Value = "EMPTY";

            else
                this.Value = fromUtf8
                    ? Encoding.UTF8.GetString(result.Data, 0, result.Data.Length)
                    : BitConverter.ToString(result.Data);
        });
    }
}
