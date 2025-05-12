using AjkAvaloniaLibs.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Remote.Protocol;
using Avalonia.Styling;
using Avalonia.VisualTree;
using DynamicData;
using DynamicData.Binding;
using ExCSS;
using ReactiveUI;
using ShimSkiaSharp;
using Svg;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        ListBox0.Background = Background;
        updateVisual();
        Nodes.CollectionChanged += Nodes_CollectionChanged;

        // Workaround for the issue where the bottom item may be hidden under the horizontal scrollbar
        // and becomes unclickable when scrolled to the lower limit.
        // Extends the scrollable range to allow further scrolling below the limit.
        Items.Add( new TreeViewItem(this) ); // add blank

        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != Avalonia.Input.KeyModifiers.Control) return;
            if (i.Delta.Y > 0) FontSize++;
            else FontSize = FontSize > 1 ? FontSize - 1 : 1;
            FontSize = FontSize;
            updateVisual();
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, true);

        this.KeyDown += TreeControl_KeyDown;

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

    private void TreeControl_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (selectedNode == null) return;
        TreeViewItem? treeItem = selectedNode.TreeItem;
        if (treeItem == null) return;

        if (e.Key == Avalonia.Input.Key.Up)
        {
            int index = Items.IndexOf(treeItem);
            index--;
            if (index < 0) return;
            TreeViewItem? item = Items[index];
            if (item == null) return;
            TreeNode? node = item.treeNode;
            if (node == null) return;
            nodeSlected(node);
        }
        else if (e.Key == Avalonia.Input.Key.Down)
        {
            int index = Items.IndexOf(treeItem);
            index++;
            if (index >= Items.Count) return;
            TreeViewItem? item = Items[index];
            if (item == null) return;
            TreeNode? node = item.treeNode;
            if (node == null) return;
            nodeSlected(node);
        }
        else if (e.Key == Avalonia.Input.Key.Left)
        {
            if (selectedNode.IsExpanded)
            {
                selectedNode.IsExpanded = false;
                treeItem.updateVisual();
            }
            else
            {
                TreeNode? parent = selectedNode.Parent;
                if (parent != null)
                {
                    nodeSlected(parent);
                }
            }
        }else if(e.Key == Avalonia.Input.Key.Right)
        {
            if(selectedNode.IsExpanded)
            {
                selectedNode.IsExpanded = false;
                treeItem.updateVisual();
            }
            else
            {
                if (selectedNode.Nodes.Count > 0)
                {
                    selectedNode.IsExpanded = true;
                    treeItem.updateVisual();
                }
            }
        }
    }

    Avalonia.Media.Color toggleButtonColor;
    public Avalonia.Media.Color ToggleButtonColor {
        get {
            return toggleButtonColor; 
        } 
        set { 
            if(toggleButtonColor == value) return;
            toggleButtonColor = value;
            updateVisual();
        } 
    }
    public Avalonia.Media.Color SelectedForegroundColor { get; set; }
    public Avalonia.Media.Color SelectedBackgroundColor { get; set; }

    internal Avalonia.Media.Imaging.Bitmap expandedIcon;
    internal Avalonia.Media.Imaging.Bitmap collaspedIcon;
    internal Avalonia.Media.Imaging.Bitmap dotIcon;

    [MemberNotNull(nameof(expandedIcon))]
    [MemberNotNull(nameof(collaspedIcon))]
    [MemberNotNull(nameof(dotIcon))]

    private void updateVisual()
    {
        expandedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/minus.svg", ToggleButtonColor);
        collaspedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/plus.svg", ToggleButtonColor);
        dotIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/dot.svg", ToggleButtonColor);

        foreach (TreeViewItem item in Items)
        {
            item.updateVisual();
        }
    }
    public ObservableCollection<TreeNode> Nodes { get; } = new ObservableCollection<TreeNode>();
    public ObservableCollection<TreeViewItem> Items { get; set; } = new ObservableCollection<TreeViewItem>();

    // call this method from all subnode nodes
    private void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TreeNode node in e.NewItems)
            {
                if (node.Nodes.Count > 0)
                {
                    System.Diagnostics.Debugger.Break();
                }
                node.parent = this;
                node.Indent = 0;
                node.updateIndent();
                node.PropertyChanged += Node_PropertyChanged;
                node.PropageteCollectionChange += PropageteCollectionChange;
            }
        }
        if (e.OldItems != null)
        {
            foreach (TreeNode node in e.OldItems)
            {
                node.parent = null;
                node.Indent = 0;
                node.updateIndent();
                node.PropertyChanged -= Node_PropertyChanged;
                node.PropageteCollectionChange -= PropageteCollectionChange;
            }
        }
        PropageteCollectionChange(this,e);
    }

    private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TreeNode treeNode) return;
        if (treeNode.TreeItem == null) return;

        treeNode.TreeItem.updateVisual();
    }
    private void PropageteCollectionChange(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Update TreeItems
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
            case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                if (e.OldItems != null)
                {
                    foreach (TreeNode node in e.OldItems)
                    {
                        removeNode(node);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (TreeNode node in e.NewItems)
                    {
                        addNode(node);
                    }
                }
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                if (sender is TreeNode ownerNode)
                {
                    TreeViewItem? ownerItem = ownerNode.TreeItem;
                    if(ownerItem != null)
                    {
                        removeAllTreeItem(ownerItem);
                    }
                }
                else if(sender is TreeControl)
                {
                    foreach (TreeViewItem item in Items)
                    {
                        TreeNode? node = item.treeNode;
                        if (node != null)
                        {
                            removeNode(node);
                        }
                    }
                    Items.Clear();
                }
                break;
        }
    }

    private void removeAllTreeItem(TreeViewItem item)
    {
        int index = Items.IndexOf(item) + 1;
        while (index < Items.Count)
        {
            TreeNode? treeNode = Items[index].treeNode;
            if (treeNode != null && treeNode.parent == item.treeNode)
            {
                removeAllTreeItem(Items[index]);
            }
            else
            {
                break;
            }
        }
        Items.Remove(item);
    }

    private void addNode(TreeNode node)
    {
        if (node._parent == null)
        {
            node._parent = new WeakReference<ITreeNodeOwner>(this);
        }
        node.ReportExpanded += NodeExpanded;
        node.ReportCollapsed += NodeCollapsed;

        // fix visuals
        if (node.Parent == null)
        {
            node.Visible = true;
        }
        else
        {
            if (node.Parent.Visible & node.Parent.IsExpanded)
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
                Items.Insert(0, new TreeViewItem(node, this));
            }
            else
            {
                int index = Items.IndexOf(Items.First(x => x.treeNode == nextTo));
                Items.Insert(index + 1, new TreeViewItem(node, this));
            }
        }

        foreach(TreeNode subNode in node.Nodes)
        {
            addNode(subNode);
        }
    }

    private void removeNode(TreeNode node)
    {
        if(selectedNode == node)
        {
            selectedNode = null;
        }

        foreach (TreeNode subNode in node.Nodes)
        {
            removeNode(subNode);
        }

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

    private void nodeSlected(TreeNode node)
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
        TreeViewItem? rootItem = node.TreeItem;
        if (rootItem == null) throw new Exception("TreeItem is null");

        int index = Items.IndexOf(rootItem) + 1;
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = true;
            TreeViewItem item = new TreeViewItem(subnode,this);
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

    public class TreeViewItem : ListBoxItem
    {
        public TreeViewItem(TreeNode node,TreeControl treeControl)
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
            TextBlock.PointerPressed += TreeItem_PointerPressed;

            DoubleTapped += TreeItem_DoubleTapped;
            StackPanel.DoubleTapped += TreeItem_DoubleTapped;
            TextBlock.DoubleTapped += TreeItem_DoubleTapped;

            node.TreeItem = this;
            updateVisual();
        }


        public TreeViewItem(TreeControl treeControl)
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
            treeControl.TreeControl_KeyDown(sender, e);
        }
        private void ToggleButton_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (treeNode == null) return;
            treeControl.nodeSlected(treeNode); // select node
            treeNode.OnClicked();
        }
        private void TreeItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (treeNode == null) return;
            treeControl.nodeSlected(treeNode); // select node
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
