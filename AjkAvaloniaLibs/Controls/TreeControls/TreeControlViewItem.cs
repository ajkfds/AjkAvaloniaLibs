using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AjkAvaloniaLibs.Controls.TreeControls
{
    // TreeView items are used to display the nodes in the ListBox
    // automatically created by the TreeNode class
    // only visible nodes are created to reduce ui update time
    public class TreeControlViewItem : ListBoxItem
    {
        // create item collesponding to a TreeNode
        public TreeControlViewItem(TreeNode node, TreeControl treeControl)
        {
            this.treeControl = treeControl;
            Content = StackPanel;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

            RenderOptions.SetBitmapInterpolationMode(Image, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);

            StackPanel.Children.Add(ToggleButton);
            StackPanel.Children.Add(Image);
            StackPanel.Children.Add(TextBlock);

            StackPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            TextBlock.Text = node.Text;
            this.treeNode = node;

            this.KeyDown += TreeNode_KeyDown;
            ToggleButton.Tapped += ToggleButton_Tapped;
            ToggleButton.PointerPressed += ToggleButton_PointerPressed;

            PointerPressed += TreeItem_PointerPressed;
            StackPanel.PointerPressed += TreeItem_PointerPressed;
            //            TextBlock.PointerPressed += TreeItem_PointerPressed;

            DoubleTapped += TreeItem_DoubleTapped;
            StackPanel.DoubleTapped += TreeItem_DoubleTapped;
            TextBlock.DoubleTapped += TreeItem_DoubleTapped;

            node.TreeControlViewItem = this;
            updateVisual();
        }

        // create blank item to add to the end of the list
        public TreeControlViewItem(TreeControl treeControl)
        {
            this.treeControl = treeControl;
            Content = StackPanel;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

            StackPanel.Children.Add(ToggleButton);
            StackPanel.Children.Add(Image);
            StackPanel.Children.Add(TextBlock);

            StackPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        }


        double? prevFontSize = null;
        bool? prevSelected = null;
        internal void updateVisual()
        {
            if (treeNode == null) return;

            if (prevFontSize != treeControl.FontSize)
            {
                StackPanel.Height = treeControl.FontSize * 1.2;
                Image.Width = treeControl.FontSize;
                Image.Height = treeControl.FontSize;
                Image.Source = treeNode.Image;
                Image.Margin = new Thickness(0, 0, treeControl.FontSize * 0.2, 0);

                ToggleButton.Margin = new Thickness(treeNode.Indent * treeControl.FontSize + treeControl.FontSize * 0.2, treeControl.FontSize * 0.2, treeControl.FontSize * 0.2, treeControl.FontSize * 0.2);

                prevFontSize = treeControl.FontSize;
            }

            if (treeNode.Nodes.Count == 0)
            {
                if (ToggleButton.Source != treeControl.dotIcon) ToggleButton.Source = treeControl.dotIcon;
            }
            else if (treeNode.IsExpanded)
            {
                if (ToggleButton.Source != treeControl.collaspedIcon) ToggleButton.Source = treeControl.collaspedIcon;
            }
            else
            {
                if (ToggleButton.Source != treeControl.expandedIcon) ToggleButton.Source = treeControl.expandedIcon;
            }

            if (treeNode.Selected)
            {
                if (prevSelected != true)
                {
                    TextBlock.Foreground = new SolidColorBrush(treeControl.SelectedForegroundColor);
                    TextBlock.Background = new SolidColorBrush(treeControl.SelectedBackgroundColor);
                    prevSelected = true;
                }
            }
            else
            {
                if (prevSelected != false)
                {
                    TextBlock.Foreground = treeControl.Foreground;
                    TextBlock.Background = treeControl.Background;
                    prevSelected = false;
                }
            }

            StackPanel.Background = treeControl.Background;

            if (TextBlock.Text != treeNode.Text)
            {
                TextBlock.Text = treeNode.Text;
            }

            if (Image.Source != treeNode.Image)
            {
                Image.Source = treeNode.Image;
            }
        }

        private TreeControl treeControl;
        private void TreeNode_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (treeNode == null) return;
            treeControl.OnKeyDown(sender, e);
        }
        private void ToggleButton_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (treeNode == null) return;
            var keyModifiers = e.KeyModifiers;
            treeControl.HandleSelection(treeNode, keyModifiers); // select node with multi-select support
        }
        private void TreeItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (treeNode == null) return;
            var keyModifiers = e.KeyModifiers;
            treeControl.HandleSelection(treeNode, keyModifiers); // select node with multi-select support
        }
        private void TreeItem_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (treeNode == null) return;
            treeNode.IsExpanded = !treeNode.IsExpanded;
        }

        private void ToggleButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (treeNode == null) return;
            if (treeNode.Nodes.Count == 0) return;
            treeNode.IsExpanded = !treeNode.IsExpanded;
            updateVisual();
            e.Handled = true;
        }

        internal TreeNode? treeNode;


        public double RowHeight
        {
            get { return StackPanel.Height; }
            set
            {
                StackPanel.Height = value;
                Image.Height = value;
                Image.Width = value;
                ToggleButton.Height = value;
                ToggleButton.Width = value;
            }
        }

        public StackPanel StackPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            MinHeight = 0
        };

        public Image ToggleButton = new Image()
        {
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            MinHeight = 0
        };

        public Image Image = new Image()
        {
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        public TextBlock TextBlock = new TextBlock()
        {
            Margin = new Thickness(0, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MinHeight = 0,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

    }
}
