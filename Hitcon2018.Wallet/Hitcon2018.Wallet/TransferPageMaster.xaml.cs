using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hitcon2018.Wallet
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransferPageMaster : ContentPage
    {
        public ListView ListView;

        public TransferPageMaster()
        {
            InitializeComponent();

            BindingContext = new TransferPageMasterViewModel();
            ListView = MenuItemsListView;
        }

        class TransferPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<TransferPageMenuItem> MenuItems { get; set; }
            
            public TransferPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<TransferPageMenuItem>(new[]
                {
                    new TransferPageMenuItem { Id = 0, Title = "Page 1" },
                    new TransferPageMenuItem { Id = 1, Title = "Page 2" },
                    new TransferPageMenuItem { Id = 2, Title = "Page 3" },
                    new TransferPageMenuItem { Id = 3, Title = "Page 4" },
                    new TransferPageMenuItem { Id = 4, Title = "Badge Wallet" },
                });
            }
            
            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}