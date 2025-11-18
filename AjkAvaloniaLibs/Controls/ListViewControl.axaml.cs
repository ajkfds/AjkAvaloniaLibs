using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Controls
{
    public partial class ListViewControl : UserControl
    {
        public ListViewControl()
        {
            InitializeComponent();

            DataContext = this;
            ListBox0.ItemsSource = this.Items;
            ListBox0.Background = Background;

            if (Design.IsDesignMode)
            {
                Items.Add(new ListViewItem("item0", Avalonia.Media.Colors.Red));
                Items.Add(new ListViewItem("item1", Avalonia.Media.Colors.Blue));
                Items.Add(new ListViewItem("item2", Avalonia.Media.Colors.Green));
                for(int i = 3; i < 100; i++)
                {
                    Items.Add(new ListViewItem("item"+i.ToString(), Avalonia.Media.Colors.Gray));
                }
            }

            {
                Style style = new Style();
                style.Selector = ((Selector?)null).OfType(typeof(ListBoxItem));
                style.Add(new Setter(Layoutable.MinHeightProperty, 1.0));

                style.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(2, 0, 2, 0)));
                style.Add(new Setter(Layoutable.MarginProperty, new Thickness(0)));

                ListBox0.Styles.Add(style);
            }

        }
        public ObservableCollection<ListViewItem> Items = new ObservableCollection<ListViewItem>();

//        public double FontSize { set; get; } = 8;
//        public double MinHeight { set; get; } = 8;
        public new double MaxHeight
        {
            set
            {
                ListBox0.MaxHeight = value;
            }
            get
            {
                return ListBox0.MaxHeight;
            }
        }

        public void Scroll(ListViewItem item)
        {
            if (!ListBox0.Items.Contains(item)) return;

            ListBox0.ScrollIntoView(item);
        }

        public int SelectedIndex
        {
            get
            {
                return ListBox0.SelectedIndex;
            }
            set
            {
                ListBox0.SelectedIndex = value;
            }
        }

        public ListViewItem? SelectedItem
        {
            get
            {
                object? item = ListBox0.SelectedItem;
                if (item == null) return null;
                return item as ListViewItem;
            }
            set
            {
                ListBox0.SelectedItem = value;
            }
        }

        public int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }

        

    }
}
