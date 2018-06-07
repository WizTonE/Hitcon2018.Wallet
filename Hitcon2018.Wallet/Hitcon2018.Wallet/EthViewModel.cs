using System;
using System.Collections.Generic;
using System.Text;

namespace Hitcon2018.Wallet
{
    public class EthViewModel : ViewModel
    {
        public EthViewModel()
        {

        }

        public ICommand ScanToggle { get; }
        public ICommand OpenSettings { get; }
        public ICommand ToggleAdapterState { get; }
        public ICommand SelectDevice { get; }
        public ObservableCollection<ScanResultViewModel> Devices { get; }
    }
}
