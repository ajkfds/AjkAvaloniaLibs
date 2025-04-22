using AjkAvaloniaLibs.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.TextInput;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using DynamicData;
using DynamicData.Binding;
using ExCSS;
using ReactiveUI;
using Svg;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AjkAvaloniaLibs.Controls;

public partial class TreeControl : UserControl,ITreeNodeOwner
{
    public TreeControl()
    {
        ToggleButtonColor = Avalonia.Media.Colors.Gray;
        SelectedBackgroundColor = Avalonia.Media.Colors.DarkGray;
        SelectedForegroundColor = Avalonia.Media.Colors.LightGray;

        InitializeComponent();
        DataContext = this;
        ListBox0.ItemsSource = this.Items;
        updateVisual();
        Nodes.CollectionChanged += Nodes_CollectionChanged;

        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != Avalonia.Input.KeyModifiers.Control) return;
            if (i.Delta.Y > 0) FontSize++;
            else FontSize = FontSize > 1 ? FontSize - 1 : 1;
            FontSize = FontSize;
            updateVisual();
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, true);

        if (Design.IsDesignMode)
        {
            TreeNode node1 = new TreeNode("TestNode1");
            TreeNode node2 = new TreeNode("TestNode2");
            TreeNode node3 = new TreeNode("TestNode3");
            TreeNode node4 = new TreeNode("TestNode4");

            TreeNode node1_1 = new TreeNode("TestNode1-1");
            TreeNode node1_2 = new TreeNode("TestNode1-2");

            TreeNode node1_1_1 = new TreeNode("TestNode1-1-1");

            TreeNode node3_1 = new TreeNode("TestNode3-1");

            Nodes.Add(node1);
            Nodes.Add(node2);
            Nodes.Add(node3);

            node1.Nodes.Add(node1_1);
            node1.Nodes.Add(node1_2);
            node1_1.Nodes.Add(node1_1_1);

            node3.Nodes.Add(node3_1);
        }
    }

    public Avalonia.Media.Color ToggleButtonColor { get; set; }
    public Avalonia.Media.Color SelectedForegroundColor { get; set; }
    public Avalonia.Media.Color SelectedBackgroundColor { get; set; }

    internal Avalonia.Media.Imaging.Bitmap expandedIcon;
    internal Avalonia.Media.Imaging.Bitmap collaspedIcon;
    internal Avalonia.Media.Imaging.Bitmap dotIcon;
    private void updateVisual()
    {
        expandedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/minus.svg", ToggleButtonColor);
        collaspedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/plus.svg", ToggleButtonColor);
        dotIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/dot.svg", ToggleButtonColor);

        foreach (TreeItem item in Items)
        {
            item.updateVisual();
        }
    }
    public ObservableCollection<TreeNode> Nodes { get; } = new ObservableCollection<TreeNode>();
    public ObservableCollection<TreeItem> Items { get; set; } = new ObservableCollection<TreeItem>();

    // call this method from all subnode nodes
    private void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        PropageteCollectionChange(this,e);
    }
    private void PropageteCollectionChange(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TreeNode node in e.NewItems)
            {
                if (node._parent == null)
                {
                    node._parent = new WeakReference<ITreeNodeOwner>(this);
                }
                node.PropageteCollectionChange += PropageteCollectionChange;
                node.ReportExpanded += NodeExpanded;
                node.ReportCollapsed += NodeCollapsed;

                // fix visuals
                if (node.Parent == null)
                {
                    node.Visible = true;
                }
                else
                {
                    if(node.Parent.Visible & node.Parent.IsExpanded)
                    {
                        node.Visible = true;
                    }
                    else
                    {
                        node.Visible = false;
                    }
                }

                if (node.Visible)
                {
                    TreeNode? nextTo = node.NextTo;
                    if (nextTo == null)
                    {
                        Items.Insert(0, new TreeItem(node,this));
                    }
                    else
                    {
                        int index = Items.IndexOf(Items.First(x => x.treeNode == nextTo));
                        Items.Insert(index + 1, new TreeItem(node,this));
                    }
                }
            }
        }
        if (e.OldItems != null)
        {
            foreach (TreeNode node in e.OldItems)
            {
                node.CollectionChanged -= Nodes_CollectionChanged;
                node._parent = null;
                node.PropageteCollectionChange -= PropageteCollectionChange;
                node.ReportExpanded -= NodeExpanded;
                node.ReportCollapsed -= NodeCollapsed;

                // fix visuals
                if (node.TreeItem != null)
                {
                    Items.Remove(node.TreeItem);
                    node.TreeItem = null;
                }
            }
        }
    }

    internal void nodeTapped(TreeNode node, Avalonia.Input.TappedEventArgs e)
    {
        if (selectedNode != null) selectedNode.Selected = false; 
        selectedNode = node;
        selectedNode.Selected = true;
        selectedNode.OnSelected();
    }
    private TreeNode? selectedNode { get; set; } = null;

    public TreeNode? GetSelectedNode()
    {
        return selectedNode;
    }
    private void NodeExpanded(TreeNode node)
    {
        TreeItem? rootItem = node.TreeItem;
        if (rootItem == null) throw new Exception("TreeItem is null");

        int index = Items.IndexOf(rootItem) + 1;
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = true;
            TreeItem item = new TreeItem(subnode,this);
            Items.Insert(index, item);
            index++;
        }
        foreach (TreeNode subnode in node.Nodes)
        {
            if (subnode.IsExpanded)
            {
                NodeExpanded(subnode);
            }
        }
    }
    private void NodeCollapsed(TreeNode node)
    {
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = false;
            if (subnode.TreeItem == null) throw new Exception("TreeItem is null");
            Items.Remove(subnode.TreeItem);
            subnode.TreeItem = null;
        }
        foreach (TreeNode subnode in node.Nodes)
        {
            if (!subnode.IsExpanded) continue;
            NodeCollapsed(subnode);
        }
    }

    public class TreeItem : ListBoxItem
    {
        public TreeItem(TreeNode node,TreeControl treeControl)
        {
            this.treeControl = treeControl;
            Content = StackPanel;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(Avalonia.Media.Colors.Transparent);

            StackPanel.Children.Add(ToggleButton);
            StackPanel.Children.Add(Image);
            StackPanel.Children.Add(TextBlock);

            StackPanel.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            TextBlock.Text = node.Text;
            this.treeNode = node;

            ToggleButton.Tapped += ToggleButton_Tapped;

            Tapped += TreeItem_Tapped;
            StackPanel.Tapped += TreeItem_Tapped;
            TextBlock.Tapped += TreeItem_Tapped;

            DoubleTapped += TreeItem_DoubleTapped;
            StackPanel.DoubleTapped += TreeItem_DoubleTapped;
            TextBlock.DoubleTapped += TreeItem_DoubleTapped;

            node.TreeItem = this;
            updateVisual();
        }

        double? prevFontSize = null;
        bool? prevSelected = null;
        internal void updateVisual()
        {
            if (treeNode == null) return;

            if(prevFontSize != treeControl.FontSize)
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
                if(ToggleButton.Source != treeControl.dotIcon) ToggleButton.Source = treeControl.dotIcon;
            }
            else if (treeNode.IsExpanded)
            {
                if(ToggleButton.Source != treeControl.collaspedIcon) ToggleButton.Source = treeControl.collaspedIcon;
            }
            else
            {
                if(ToggleButton.Source != treeControl.expandedIcon) ToggleButton.Source = treeControl.expandedIcon;
            }

            if (treeNode.Selected)
            {
                if(prevSelected != true)
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
        }

        private TreeControl treeControl;
        private void TreeItem_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (treeNode == null) return;
            treeControl.nodeTapped(treeNode, e); // select node
            treeNode.OnClicked();
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

        internal TreeNode treeNode;


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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
//            Background = new SolidColorBrush(Avalonia.Media.Colors.Transparent)
        };

    }


}
