using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Contorls
{
    public partial class ListViewControl : UserControl
    {
        public ListViewControl()
        {
            InitializeComponent();
            ListViewControlViewModel vm = new ListViewControlViewModel();

            ListBox.DataContext = vm;
            ListBox.ItemsSource = vm.Items;
        }

        public ObservableCollection<ListViewItem> Items
        {
            get
            {
                return ((ListViewControlViewModel)ListBox.DataContext).Items;
            }
        }


    }
}
