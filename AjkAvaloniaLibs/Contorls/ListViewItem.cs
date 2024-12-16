using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static AjkAvaloniaLibs.Contorls.TreeNode;
using static System.Net.Mime.MediaTypeNames;

namespace AjkAvaloniaLibs.Contorls
{
    public class ListViewItem : INotifyPropertyChanged
    {
        public ListViewItem()
        {
            ForeColor = Colors.Gray;
            BackColor = Colors.Transparent;
        }

        public ListViewItem(string text) : this()
        {
            Text = text;
        }

        public ListViewItem(string text,Color foreColor) : base()
        {
            Text = text;
            ForeColor = foreColor;
        }

        const int brushCashLimit = 50;
        public static Dictionary<Color, SolidColorBrush> brushes = new Dictionary<Color, SolidColorBrush>();
        private SolidColorBrush getBrush(Color color)
        {
            lock (brushes)
            {
                if (brushes.ContainsKey(color)) return brushes[color];
                if (brushes.Count >= brushCashLimit) brushes.Remove(brushes.Keys.First());
                SolidColorBrush brush = new SolidColorBrush(color);
                brushes.Add(color,brush);
                return brush;
            }
        }


        public Avalonia.Media.SolidColorBrush ForeColorBrush
        {
            get
            {
                return getBrush(ForeColor);
            }
        }
        public Avalonia.Media.SolidColorBrush BackColorBrush
        {
            get
            {
                return getBrush(BackColor);
            }
        }

        //public int Height { get; set; }
        //public int FontSize { get; set; }


        public Avalonia.Media.Color ForeColor { get; set; } 
        public Avalonia.Media.Color BackColor { get; set; } 

        private string _Text = "";
        public virtual string Text
        {
            get { return _Text; }
            set { _Text = value; NotifyPropertyChanged(); }
        }



        // 双方向BIndingのためのViewModelへのProperty変更通知
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
