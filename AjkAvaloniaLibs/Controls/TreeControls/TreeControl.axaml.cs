using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Threading;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AjkAvaloniaLibs.Controls.TreeControls;

public partial class TreeControl : UserControl, ITreeNodeOwner, INotifyPropertyChanged
{
    public TreeControl()
    {
        ToggleButtonColor = Avalonia.Media.Colors.Gray;
        SelectedBackgroundColor = Avalonia.Media.Colors.DarkGray;
        SelectedForegroundColor = Avalonia.Media.Colors.LightGray;

        InitializeComponent();
        DataContext = this;
//        ListBox0.ItemsSource = this.Items;
        ListBox0[!ListBox.ItemsSourceProperty] = new Binding(nameof(Items));


        ListBox0.Background = Background;
        updateVisual();
        Nodes.CollectionChanged += Nodes_CollectionChanged;

        // Workaround for the issue where the bottom item may be hidden under the horizontal scrollbar
        // and becomes unclickable when scrolled to the lower limit.
        // Extends the scrollable range to allow further scrolling below the limit.
        Items.Add(new TreeControlViewItem(this)); // add blank

        // scroll handler
        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != Avalonia.Input.KeyModifiers.Control) return;
            if (i.Delta.Y > 0) FontSize++;
            else FontSize = FontSize > 1 ? FontSize - 1 : 1;
            FontSize = FontSize;
            if (OnFontSizeChanged != null) OnFontSizeChanged(FontSize);
            updateVisual();
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, true);

        this.KeyDown += OnKeyDown;

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

    public Avalonia.Media.Color SelectedForegroundColor { get; set; }
    public Avalonia.Media.Color SelectedBackgroundColor { get; set; }


    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => _propertyChanged += value;
        remove => _propertyChanged -= value;
    }
    private PropertyChangedEventHandler? _propertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Root Node -----------------------------------

    private ObservableCollection<TreeNode> nodes = new ObservableCollection<TreeNode>();
    public ObservableCollection<TreeNode> Nodes
    {
        get { return nodes; }
        set
        {
            nodes.CollectionChanged -= Nodes_CollectionChanged;
            AllNodesChanged(this);
            Nodes = value;
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }
    }
    public int Indent { get { return 0; } set { } }

    public bool IsExpanded { get; } = true;
    public bool Visible { get; } = true;

    // Trigger from nodes

    internal void AllNodesChanged(ITreeNodeOwner nodeOwner)
    {
        updateAllTreeViewItems();
        updateVisual();
    }

    internal void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        updateAllTreeViewItems();
        updateVisual();
        return;
        if (!Dispatcher.UIThread.CheckAccess() && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();

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
                node.UpdateIndent(node);
            }
        }
        if (e.OldItems != null)
        {
            foreach (TreeNode node in e.OldItems)
            {
                node.parent = null;
                node.Indent = 0;
                node.UpdateIndent(node);
            }
        }
    }

    internal void NodeExpanded(TreeNode node)
    {
        updateAllTreeViewItems();
        updateVisual();
        return;

        TreeControlViewItem? rootItem = node.TreeItem;
        if (rootItem == null) throw new Exception("TreeItem is null");

        int index = Items.IndexOf(rootItem) + 1;
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = true;
            subnode.Indent = node.Indent + 1;
            TreeControlViewItem item = new TreeControlViewItem(subnode, this);
            Items.Insert(index, item);
            index++;

            // Process already-expanded grandchildren recursively
            if (subnode.IsExpanded)
            {
                InsertExpandedSubtree(subnode, ref index);
            }
        }
    }
    internal void NodeCollapsed(TreeNode node)
    {
        updateAllTreeViewItems();
        updateVisual();
        return;
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = false;
            if (subnode.TreeItem == null)
            {
                //if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
            else
            {
                Items.Remove(subnode.TreeItem);
                subnode.TreeItem = null;
            }
        }
        foreach (TreeNode subnode in node.Nodes)
        {
            if (!subnode.IsExpanded) continue;
            NodeCollapsed(subnode);
        }
    }



    // Control -------------------------------------

    public Action<double>? OnFontSizeChanged = null;

    // マルチ選択用
    private HashSet<TreeNode> selectedNodes = new HashSet<TreeNode>();
    private TreeNode? lastSelectedNodeForShift = null;

    internal void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (selectedNode == null) return;
        TreeControlViewItem? treeItem = selectedNode.TreeItem;
        if (treeItem == null) return;

        if (e.Key == Avalonia.Input.Key.Up)
        {
            int index = Items.IndexOf(treeItem);
            index--;
            if (index < 0) return;
            TreeControlViewItem? item = Items[index];
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
            TreeControlViewItem? item = Items[index];
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
                if(selectedNode.Parent is TreeNode)
                {
                    TreeNode? parent = (TreeNode)selectedNode.Parent;
                    if (parent != null)
                    {
                        nodeSlected(parent);
                    }
                }
            }
        }
        else if (e.Key == Avalonia.Input.Key.Right)
        {
            if (selectedNode.IsExpanded)
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
    public Avalonia.Media.Color ToggleButtonColor
    {
        get
        {
            return toggleButtonColor;
        }
        set
        {
            if (toggleButtonColor == value) return;
            toggleButtonColor = value;
            updateVisual();
        }
    }


    internal Avalonia.Media.Imaging.Bitmap expandedIcon { get; set; } = null!;
    internal Avalonia.Media.Imaging.Bitmap collaspedIcon { get; set; } = null!;
    internal Avalonia.Media.Imaging.Bitmap dotIcon { get; set; } = null!;

    private void updateVisual()
    {
        expandedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/minus.svg", ToggleButtonColor);
        collaspedIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/plus.svg", ToggleButtonColor);
        dotIcon = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/dot.svg", ToggleButtonColor);

        foreach (TreeControlViewItem item in Items)
        {
            item.updateVisual();
        }
    }

    public ObservableCollection<TreeControlViewItem> _items = new ObservableCollection<TreeControlViewItem>();

    public ObservableCollection<TreeControlViewItem> Items
    {
        get
        {
            return _items;
        }
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }


    private void updateAllTreeViewItems()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }
        ObservableCollection<TreeControlViewItem> items = new ObservableCollection<TreeControlViewItem>();
        updateSubTreeViewItems(items, this);

        items.Add(new TreeControlViewItem(this)); // add blank

        //Items.Clear();
        //foreach (var item in items)
        //{
        //    Items.Add(item);
        //}
        Items = items;
    }

    private void updateSubTreeViewItems(ObservableCollection<TreeControlViewItem> items,ITreeNodeOwner owner)
    {
        foreach(TreeNode treeNode in owner.Nodes)
        {
            treeNode.treeControl = this;
            treeNode.parent = owner;
            TreeControlViewItem item = new TreeControlViewItem(treeNode, this);
            items.Add(item);
            if (treeNode.IsExpanded)
            {
                updateSubTreeViewItems(items, treeNode);
            }
        }
    }

    // call this method from all subnodes

    /// <summary>
    /// Batch update all visible TreeControlViewItems under this TreeControl.
    /// Called when collection changes occur to update all affected nodes at once.
    /// </summary>
    //internal void BatchUpdateVisual()
    //{
    //    foreach (TreeControlViewItem item in Items)
    //    {
    //        item.updateVisual();
    //    }
    //}

    //private void PropageteCollectionChange(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    // Update TreeItems
    //    switch (e.Action)
    //    {
    //        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
    //        case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
    //        case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
    //        case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
    //            if (e.OldItems != null)
    //            {
    //                foreach (TreeNode node in e.OldItems)
    //                {
    //                    removeNode(node);
    //                }
    //            }
    //            if (e.NewItems != null)
    //            {
    //                foreach (TreeNode node in e.NewItems)
    //                {
    //                    addNode(node);
    //                }
    //            }
    //            break;
    //        case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
    //            if (sender is TreeNode ownerNode)
    //            {
    //                // Remove all child TreeControlViewItems recursively, starting with direct children
    //                foreach (TreeNode childNode in new List<TreeNode>(ownerNode.Nodes))
    //                {
    //                    removeNode(childNode);
    //                }
    //                TreeControlViewItem? ownerItem = ownerNode.TreeItem;
    //                if (ownerItem != null)
    //                {
    //                    removeAllTreeItem(ownerItem);
    //                }
    //                // Add new child nodes (if any)
    //                foreach (TreeNode newNode in ownerNode.Nodes)
    //                {
    //                    addNode(newNode);
    //                }
    //            }
    //            else if (sender is TreeControl)
    //            {
    //                foreach (TreeControlViewItem item in Items)
    //                {
    //                    TreeNode? node = item.treeNode;
    //                    if (node != null)
    //                    {
    //                        removeNode(node);
    //                    }
    //                }
    //                Items.Clear();
    //                // Add new root nodes (if any)
    //                foreach (TreeNode newNode in Nodes)
    //                {
    //                    addNode(newNode);
    //                }
    //            }
    //            // Batch update all visible TreeControlViewItems after Reset
    //            BatchUpdateVisual();
    //            break;
    //    }
    //}

    //private void removeAllTreeItem(TreeControlViewItem item)
    //{
    //    int index = Items.IndexOf(item) + 1;
    //    while (index < Items.Count)
    //    {
    //        TreeNode? treeNode = Items[index].treeNode;
    //        if (treeNode != null && treeNode.parent == item.treeNode)
    //        {
    //            removeAllTreeItem(Items[index]);
    //        }
    //        else
    //        {
    //            break;
    //        }
    //    }
    //    Items.Remove(item);
    //}

    //private void addNode(TreeNode node)
    //{
    //    if (System.Diagnostics.Debugger.IsAttached & !Dispatcher.UIThread.CheckAccess()) System.Diagnostics.Debugger.Break();

    //    if (node._parent == null)
    //    {
    //        node._parent = new WeakReference<ITreeNodeOwner>(this);
    //    }
    //    node.treeControl = this;

    //    // Calculate visibility
    //    if (node.Parent == null)
    //    {
    //        node.Visible = true;
    //    }
    //    else
    //    {
    //        node.Visible = node.Parent.Visible && node.Parent.IsExpanded;
    //    }

    //    if (!node.Visible)
    //    {
    //        // Node is not visible, but we still need to create TreeControlViewItem
    //        // so that when parent expands, the node will be shown correctly
    //        if (node.TreeItem == null)
    //        {
    //            new TreeControlViewItem(node, this);
    //        }
    //        return;
    //    }

    //    // Find position to insert
    //    if (!node.GetNextTo(out TreeNode? nextTo))
    //    {
    //        // NextTo calculation failed - fall back to parent-based insertion
    //    }
    //    if (nextTo != null)
    //    {
    //        TreeControlViewItem? nextToItem = Items.FirstOrDefault(x => x.treeNode == nextTo);
    //        if (nextToItem != null)
    //        {
    //            int index = Items.IndexOf(nextToItem);
    //            Items.Insert(index + 1, new TreeControlViewItem(node, this));
    //            return;
    //        }
    //    }

    //    // NextTo not found in Items - check parent chain for valid insertion point
    //    ITreeNodeOwner? parent = node.Parent;
    //    while (parent is TreeNode)
    //    {
    //        TreeNode parentNode = (TreeNode)parent;

    //        if (parentNode.TreeItem != null && Items.Contains(parentNode.TreeItem))
    //        {
    //            // Found valid parent in Items - insert after it
    //            int parentIndex = Items.IndexOf(parentNode.TreeItem);
    //            Items.Insert(parentIndex + 1, new TreeControlViewItem(node, this));
    //            return;
    //        }
    //        parent = parentNode.Parent;
    //    }

    //    // No ancestor found in Items - this is a root node or all ancestors are collapsed
    //    // Find root TreeNode to use as insertion reference
    //    //ITreeNodeOwner? treeNodeOwner = node.parent;
    //    //while (treeNodeOwner != null && treeNodeOwner.Parent != null)
    //    //{
    //    //    treeNodeOwner = treeNodeOwner.Parent;
    //    //}
    //    //if (treeNodeOwner is TreeNode rootNode)
    //    //{

    //    //}


    //    //if (rootNode != null && rootNode.TreeItem != null && Items.Contains(rootNode.TreeItem))
    //    //{
    //    //    // Insert after root node
    //    //    int rootIndex = Items.IndexOf(rootNode.TreeItem);
    //    //    Items.Insert(rootIndex + 1, new TreeControlViewItem(node, this));
    //    //}
    //    //else
    //    //{
    //    //    // No valid insertion point found - create TreeControlViewItem anyway (for later use)
    //    //    new TreeControlViewItem(node, this);
    //    //}
    //}


    //private void removeNode(TreeNode node)
    //{
    //    if (System.Diagnostics.Debugger.IsAttached & !Dispatcher.UIThread.CheckAccess()) System.Diagnostics.Debugger.Break();
    //    if (selectedNode == node)
    //    {
    //        selectedNode = null;
    //    }

    //    foreach (TreeNode subNode in node.Nodes)
    //    {
    //        removeNode(subNode);
    //    }

    //    // fix visuals
    //    List<TreeControlViewItem> removeItems = new List<TreeControlViewItem>();
    //    foreach (TreeControlViewItem? item in Items)
    //    {
    //        if (item.treeNode == node)
    //        {
    //            removeItems.Add(item);
    //        }
    //    }

    //    foreach (TreeControlViewItem removeItem in removeItems)
    //    {
    //        Items.Remove(removeItem);
    //    }
    //    node.TreeItem = null;

    //    //if (node.TreeItem != null)
    //    //{
    //    //    Items.Remove(node.TreeItem);
    //    //    node.TreeItem = null;
    //    //}
    //}

    public void SelectNode(TreeNode node)
    {
        if (System.Diagnostics.Debugger.IsAttached & !Dispatcher.UIThread.CheckAccess()) System.Diagnostics.Debugger.Break();
        nodeSlected(node);
    }

    // マルチ選択対応 selection handler
    public void HandleSelection(TreeNode node, KeyModifiers keyModifiers)
    {
        if (System.Diagnostics.Debugger.IsAttached & !Dispatcher.UIThread.CheckAccess()) System.Diagnostics.Debugger.Break();
        if (keyModifiers.HasFlag(KeyModifiers.Control))
        {
            // Ctrl+Click: 選択反転
            ToggleNodeSelection(node);
            lastSelectedNodeForShift = node;
        }
        else if (keyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Shift+Click: 範囲選択（同一tree階層のみ）
            if (lastSelectedNodeForShift != null)
            {
                SelectRange(lastSelectedNodeForShift, node);
            }
            else
            {
                // 最初の場合は通常の単一選択
                ClearSelection();
                AddSingleSelection(node);
            }
        }
        else
        {
            // 通常クリック: 選択解除して選択
            ClearSelection();
            AddSingleSelection(node);
            lastSelectedNodeForShift = node;
        }
    }

    private void ToggleNodeSelection(TreeNode node)
    {
        if (System.Diagnostics.Debugger.IsAttached & !Dispatcher.UIThread.CheckAccess()) System.Diagnostics.Debugger.Break();
        if (selectedNodes.Contains(node))
        {
            selectedNodes.Remove(node);
            node.Selected = false;
            node.OnDeSelected();
        }
        else
        {
            selectedNodes.Add(node);
            node.Selected = true;
            node.OnSelected();
        }
        UpdatePrimarySelection(node);
    }

    private void SelectRange(TreeNode startNode, TreeNode endNode)
    {
        // Itemsコレクションから同一階層のノードを取得
        var startIndex = -1;
        var endIndex = -1;
        int? startIndent = null;

        // まず開始ノードと同じ階層のノードを見つける
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.treeNode == startNode)
            {
                startIndex = i;
                startIndent = startNode.Indent;
                break;
            }
        }

        if (startIndex < 0 || startIndent == null) return;

        // 終了ノードを探す（同階層）
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.treeNode == endNode && item.treeNode.Indent == startIndent)
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex < 0) return;

        // 範囲内のノードを選択（同じindentレベルのものだけ）
        for (int i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
        {
            var item = Items[i];
            if (item.treeNode != null && item.treeNode.Indent == startIndent)
            {
                if (!selectedNodes.Contains(item.treeNode))
                {
                    selectedNodes.Add(item.treeNode);
                    item.treeNode.Selected = true;
                    item.treeNode.OnSelected();
                }
            }
        }

        UpdatePrimarySelection(endNode);
    }

    private void ClearSelection()
    {
        foreach (var node in selectedNodes.ToList())
        {
            node.Selected = false;
            node.OnDeSelected();
        }
        selectedNodes.Clear();
    }

    private void AddSingleSelection(TreeNode node)
    {
        selectedNodes.Clear();
        selectedNodes.Add(node);
        selectedNode = node;
        node.Selected = true;
        node.OnSelected();
    }

    private void UpdatePrimarySelection(TreeNode node)
    {
        selectedNode = node;
    }

    private void nodeSlected(TreeNode node)
    {
        if (selectedNode == node) return;
        if (selectedNode != null)
        {
            selectedNode.Selected = false;
            selectedNode.OnDeSelected();
        }
        selectedNode = node;
        selectedNode.Selected = true;
        selectedNode.OnSelected();
    }
    private TreeNode? selectedNode { get; set; } = null;

    public TreeNode? GetSelectedNode()
    {
        return selectedNode;
    }

    public IReadOnlyCollection<TreeNode> GetSelectedNodes()
    {
        return selectedNodes;
    }

    private void InsertExpandedSubtree(TreeNode node, ref int insertIndex)
    {
        foreach (TreeNode subnode in node.Nodes)
        {
            subnode.Visible = true;
            subnode.Indent = node.Indent + 1;
            TreeControlViewItem item = new TreeControlViewItem(subnode, this);
            Items.Insert(insertIndex, item);
            insertIndex++;

            if (subnode.IsExpanded)
            {
                InsertExpandedSubtree(subnode, ref insertIndex);
            }
        }
    }



}
