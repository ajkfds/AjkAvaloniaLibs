using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AjkAvaloniaLibs.Contorls
{
    public class ListViewControlViewModel
    {
        public ListViewControlViewModel()
        {
            if (Design.IsDesignMode)
            {
                Items.Add(new ListViewItem("AAA"));
                Items.Add(new ListViewItem("ABC"));
            }
        }

        public ObservableCollection<ListViewItem> Items = new ObservableCollection<ListViewItem>();
    }
}
