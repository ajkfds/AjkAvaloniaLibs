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
        ListBox0[!ListBox.ItemsSourceProperty] = new Binding(nameof(Items)) { Source = this };

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
    }

    internal void NodeExpanded(TreeNode node)
    {
        updateAllTreeViewItems();
        updateVisual();
    }
    internal void NodeCollapsed(TreeNode node)
    {
        updateAllTreeViewItems();
        updateVisual();
    }



    // Control -------------------------------------

    public Action<double>? OnFontSizeChanged = null;

    // マルチ選択用
    private HashSet<TreeNode> selectedNodes = new HashSet<TreeNode>();
    private TreeNode? lastSelectedNodeForShift = null;

    internal void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (selectedNode == null) return;
        TreeControlViewItem? treeItem = selectedNode.TreeControlViewItem;
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

    private ObservableCollection<TreeControlViewItem> _items = new();

    // Avaloniaにプロパティを登録する
    public static readonly DirectProperty<TreeControl, ObservableCollection<TreeControlViewItem>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<TreeControl, ObservableCollection<TreeControlViewItem>>(
            nameof(Items),
            o => o.Items,
            (o, v) => o.Items = v);

    public ObservableCollection<TreeControlViewItem> Items
    {
        get => _items;
        set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    private void updateAllTreeViewItems()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
        }

        // 1. 既存のアイテムを辞書にキャッシュ（TreeNode または ブランク用の this をキーにする）
        // インスタンスを使い回すことで、ポインターイベントの消失を防ぎます
        var existingItemsMap = new Dictionary<object, TreeControlViewItem>();
        foreach (var oldItem in Items)
        {
            if (oldItem.treeNode != null)
            {
                existingItemsMap[oldItem.treeNode] = oldItem;
                oldItem.treeNode.Visible = false;
            }
            else
            {
                // treeNodeがnullのものは末尾のブランクアイテムとみなす
                existingItemsMap[this] = oldItem;
            }
        }

        // 2. 本来表示されるべき「最新の正しい並び順」のリストを構築する（既存インスタンスは再利用）
        List<TreeControlViewItem> expectedItems = new List<TreeControlViewItem>();
        updateSubTreeViewItemsIncremental(expectedItems, this, existingItemsMap);

        // 末尾のブランクアイテムの追加・再利用
        if (existingItemsMap.TryGetValue(this, out var blankItem))
        {
            expectedItems.Add(blankItem);
        }
        else
        {
            expectedItems.Add(new TreeControlViewItem(this)); // add blank to keep scroll margin
        }

        // 高速判定用のハッシュセットを作成
        var expectedSet = new HashSet<TreeControlViewItem>(expectedItems);

        // 3. 既存の Items コレクションを expectedItems と完全に一致するように差分更新する
        int currentIdx = 0;
        int expectedIdx = 0;

        while (expectedIdx < expectedItems.Count)
        {
            var expectedItem = expectedItems[expectedIdx];

            if (currentIdx < Items.Count)
            {
                var currentItem = Items[currentIdx];

                if (currentItem == expectedItem)
                {
                    // インスタンスが一致していればそのまま進む
                    currentIdx++;
                    expectedIdx++;
                }
                else
                {
                    // 不一致の場合、現在の要素が「新しいリスト」に生き残っているか確認
                    if (!expectedSet.Contains(currentItem))
                    {
                        // 新しいリストに残っていない ＝ 完全に削除された要素なので Remove
                        Items.RemoveAt(currentIdx);
                        // 削除されると次の要素が currentIdx に詰まるため、インデックスは進めない
                    }
                    else
                    {
                        // 新しいリストのどこか後ろに登場する ＝ ここに期待される要素を挿入
                        Items.Insert(currentIdx, expectedItem);
                        currentIdx++;
                        expectedIdx++;
                    }
                }
            }
            else
            {
                // 既存の Items が尽きたら、残りの期待されるアイテムを末尾に追加
                Items.Add(expectedItem);
                currentIdx++;
                expectedIdx++;
            }
        }

        // 4. 既存の Items の方に余分な古い要素が残っていれば末尾から削除
        while (Items.Count > expectedItems.Count)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }

    // 差分更新用にインスタンスのキャッシュを受け取る再帰メソッド
    private void updateSubTreeViewItemsIncremental(
        List<TreeControlViewItem> expectedItems,
        ITreeNodeOwner owner,
        Dictionary<object, TreeControlViewItem> cache)
    {
        foreach (TreeNode treeNode in owner.Nodes)
        {
            treeNode.treeControl = this;
            treeNode.parent = owner;
            treeNode.Visible = true;
            treeNode.UpdateIndent(owner);

            // 既存のインスタンスがあれば再利用し、なければ新規作成する
            if (!cache.TryGetValue(treeNode, out var item))
            {
                item = new TreeControlViewItem(treeNode, this);
            }
            expectedItems.Add(item);

            if (treeNode.IsExpanded)
            {
                updateSubTreeViewItemsIncremental(expectedItems, treeNode, cache);
            }
        }
    }
    //private void updateAllTreeViewItems()
    //{
    //    if (!Dispatcher.UIThread.CheckAccess())
    //    {
    //        if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
    //    }
    //    foreach(var oldItem in Items)
    //    {
    //        if (oldItem.treeNode != null) oldItem.treeNode.Visible = false;
    //    }

    //    ObservableCollection<TreeControlViewItem> items = new ObservableCollection<TreeControlViewItem>();
    //    updateSubTreeViewItems(items, this);

    //    items.Add(new TreeControlViewItem(this)); // add blank to keep scroll margin
    //    Items = items;
    //}

    //private void updateSubTreeViewItems(ObservableCollection<TreeControlViewItem> items,ITreeNodeOwner owner)
    //{
    //    foreach(TreeNode treeNode in owner.Nodes)
    //    {
    //        treeNode.treeControl = this;
    //        treeNode.parent = owner;
    //        treeNode.Visible = true;

    //        TreeControlViewItem? item = new TreeControlViewItem(treeNode, this);
    //        items.Add(item);

    //        if (treeNode.IsExpanded)
    //        {
    //            updateSubTreeViewItems(items, treeNode);
    //        }
    //    }
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
//                ClearSelection();
                AddSingleSelection(node);
            }
        }
        else
        {
            // 通常クリック: 選択解除して選択
//            ClearSelection();
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

    //private void ClearSelection()
    //{
    //    foreach (var node in selectedNodes.ToList())
    //    {
    //        node.Selected = false;
    //        node.OnDeSelected();
    //    }
    //    selectedNodes.Clear();
    //}

    private void AddSingleSelection(TreeNode node)
    {
        foreach (var oldnode in selectedNodes.ToList())
        {
            if (oldnode == node) continue;
            oldnode.Selected = false;
            oldnode.OnDeSelected();
        }
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

}
