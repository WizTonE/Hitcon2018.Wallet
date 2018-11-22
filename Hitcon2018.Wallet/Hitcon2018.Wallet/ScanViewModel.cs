using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using Plugin.BluetoothLE;
using ReactiveUI;

namespace Hitcon2018.Wallet
{
    public class ScanViewModel : ViewModel
    {
        IDisposable scan;


        public ScanViewModel()
        {
            this.Devices = new ObservableCollection<ScanResultViewModel>();

            CrossBleAdapter
                .Current
                .WhenStatusChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(this.Title)));

            this.SelectDevice = ReactiveCommand.CreateFromTask<ScanResultViewModel>(async x =>
            {
                this.scan?.Dispose();
                await App.Current.MainPage.Navigation.PushAsync(new DevicePage
                {
                    BindingContext = new DeviceViewModel(x.Device)
                });
            });

            this.OpenSettings = ReactiveCommand.Create(() =>
            {
                if (CrossBleAdapter.Current.Features.HasFlag(AdapterFeatures.OpenSettings))
                {
                    CrossBleAdapter.Current.OpenSettings();
                }
                else
                {
                    UserDialogs.Instance.Alert("Cannot open bluetooth settings");
                }
            });

            this.ToggleAdapterState = ReactiveCommand.Create(
                () =>
                {
                    if (CrossBleAdapter.Current.CanControlAdapterState())
                    {
                        var poweredOn = CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn;
                        CrossBleAdapter.Current.SetAdapterState(!poweredOn);
                    }
                    else
                    {
                        UserDialogs.Instance.Alert("Cannot change bluetooth adapter state");
                    }
                }
            );

            this.ScanToggle = ReactiveCommand.Create(
                async () =>
                {
                    if (this.IsScanning)
                    {
                        this.scan?.Dispose();
                        this.IsScanning = false;
                    }
                    else
                    {
                        this.Devices.Clear();
                        var walletSettings = await App.Database.BadgeDAO.GetItemsAsync();
                        var walletSetting = walletSettings.Last();
                        var badgeSeviceUuid = walletSetting != null ? new Guid(walletSetting.ServiceUUID) : default(Guid);
                        this.IsScanning = true;
                        this.scan = CrossBleAdapter
                            .Current
                            .Scan()
                            .Buffer(TimeSpan.FromSeconds(0.5))
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(results =>
                            {
                                foreach (var result in results)
                                { 
                                    try
                                    {
                                        if (result.AdvertisementData.ServiceUuids.Length > 0)
                                            this.OnScanResult(result);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }

                                IScanResult findBadge = null;
                                foreach (var item in results)
                                {
                                    try
                                    {
                                        if (item.AdvertisementData.ServiceUuids.Length > 0 && item.AdvertisementData.ServiceUuids[0].Equals(badgeSeviceUuid))
                                        {
                                            findBadge = item;
                                            Badge.Instance.BleDevice = item.Device;
                                            break;
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }

                                if (findBadge != null)
                                {
                                    //findBadge = new ScanResult(new Device)
                                    this.scan?.Dispose();
                                    this.IsScanning = false;
                                    //App.Current.MainPage.Navigation.PushAsync(new DevicePage
                                    //{
                                    //    BindingContext = new DeviceViewModel(findBadge.Device)
                                    //});
                                    Badge.Instance.ConnectToBadge();
                                    App.Current.MainPage.Navigation.PopAsync();
                                }
                                
                            });
                    }
                },
                CrossBleAdapter
                    .Current
                    .WhenStatusChanged()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(x => x == AdapterStatus.PoweredOn)
            );
        }


        public override void OnDeactivated()
        {
            base.OnDeactivated();
            this.scan?.Dispose();
        }


        public ICommand ScanToggle { get; }
        public ICommand OpenSettings { get; }
        public ICommand ToggleAdapterState { get; }
        public ICommand SelectDevice { get; }
        public ObservableCollection<ScanResultViewModel> Devices { get; }


        public string Title => $"{CrossBleAdapter.Current.DeviceName} ({CrossBleAdapter.Current.Status})";


        bool scanning;
        public bool IsScanning
        {
            get => this.scanning;
            private set => this.RaiseAndSetIfChanged(ref this.scanning, value);
        }


        void OnScanResult(IScanResult result)
        {
            var dev = this.Devices.FirstOrDefault(x => x.Uuid.Equals(result.Device.Uuid));
            if (dev != null)
            {
                dev.TrySet(result);
            }
            else
            {
                dev = new ScanResultViewModel();
                dev.TrySet(result);
                this.Devices.Add(dev);
            }
        }
    }
}
