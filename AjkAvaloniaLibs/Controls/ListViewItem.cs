using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AjkAvaloniaLibs.Controls
{
    public class ListViewItem : ListBoxItem
    {
        public ListViewItem(string text):this()
        {
            Text = text;
        }

        public ListViewItem(string text, Color foreColor) : this()
        {
            Text = text;
            ForeColor = foreColor;
        }
        public ListViewItem()
        {
            Content = StackPanel;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

            RenderOptions.SetBitmapInterpolationMode(IconImage, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);

            StackPanel.Children.Add(IconImage);
            StackPanel.Children.Add(TextBlock);
            StackPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            //this.KeyDown += TreeNode_KeyDown;
            //ToggleButton.Tapped += ToggleButton_Tapped;
            //ToggleButton.PointerPressed += ToggleButton_PointerPressed;

            //PointerPressed += TreeItem_PointerPressed;
            //StackPanel.PointerPressed += TreeItem_PointerPressed;
            ////            TextBlock.PointerPressed += TreeItem_PointerPressed;

            //DoubleTapped += TreeItem_DoubleTapped;
            //StackPanel.DoubleTapped += TreeItem_DoubleTapped;
            //TextBlock.DoubleTapped += TreeItem_DoubleTapped;

            updateVisual();

            PropertyChanged += ListViewItem_PropertyChanged;
        }

        private void ListViewItem_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            updateVisual();
        }

        public StackPanel StackPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        public Avalonia.Controls.Image IconImage = new Avalonia.Controls.Image()
        {
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        public TextBlock TextBlock = new TextBlock()
        {
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };


        //public int Height { get; set; }
        //public int FontSize { get; set; }

        public Avalonia.Media.Color ForeColor { get; set; } = Avalonia.Media.Colors.White;

        public virtual string Text { get; set; } = "";

        public new double FontSize { get; set; } = 8;

        public Avalonia.Media.IImage? Image { get; set; }

        public Avalonia.Media.Color SelectedForegroundColor { get; set; }= Avalonia.Media.Colors.White;

        public Avalonia.Media.Color SelectedBackgroundColor { get; set; } = Avalonia.Media.Colors.DarkBlue;
        internal void updateVisual()
        {
            StackPanel.Height = FontSize * 1.2;
            TextBlock.FontSize = FontSize;
            //IconImage.Width = FontSize;
            //IconImage.Height = FontSize;
            //IconImage.Source = Image;
            //IconImage.Margin = new Thickness(0, 0, FontSize * 0.2, 0);

            if (IsSelected)
            {
                TextBlock.Foreground = new SolidColorBrush(SelectedForegroundColor);
                TextBlock.Background = new SolidColorBrush(SelectedBackgroundColor);
                StackPanel.Background = new SolidColorBrush(SelectedBackgroundColor);
            }
            else
            {
                TextBlock.Foreground = new SolidColorBrush(ForeColor);
                TextBlock.Background = null;
                StackPanel.Background = null;
            }


            TextBlock.Text = Text;

            //IconImage.Source = Image;
        }
    }
}
