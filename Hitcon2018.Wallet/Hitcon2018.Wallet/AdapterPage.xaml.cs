using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hitcon2018.Wallet
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdapterPage : TabbedPage
    {
        public IDevice Device { get; set; }
        public AdapterPage ()
        {
            InitializeComponent();
        }

        public void DeviceReady(IDevice device)
        {
            Device = device;
        }
    }
}