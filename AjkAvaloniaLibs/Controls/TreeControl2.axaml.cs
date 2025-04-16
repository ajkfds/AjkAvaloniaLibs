using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AjkAvaloniaLibs.Controls;

public partial class TreeControl2 : UserControl
{
    public TreeControl2()
    {
        InitializeComponent();
        DataContext = viewModel;

        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != KeyModifiers.Control) return;
            if (i.Delta.Y > 0) FontSize++;
            else FontSize = FontSize > 1 ? FontSize - 1 : 1;
            viewModel.FontSize = FontSize;
        }, RoutingStrategies.Bubble, true);

        Tree.ApplyTemplate();
        var headersPresenter = Tree.FindControl<Control>("PART_ColumnHeadersPresenter");
        if (headersPresenter != null)
        {
            headersPresenter.IsVisible = false;
        }

        Tree.ShowColumnHeaders = false;
        Tree.Tapped += Tree_Tapped;
        Tree.DoubleTapped += Tree_DoubleTapped;

        viewModel.NodesSource.RowExpanded += NodesSource_RowExpanded;
        viewModel.NodesSource.RowCollapsed += NodesSource_RowCollapsed;

        if (Tree.RowSelection != null)
        {
            Tree.RowSelection.SelectionChanged += RowSelection_SelectionChanged;
        }
    }



    private void NodesSource_RowCollapsed(object? sender, Avalonia.Controls.Models.TreeDataGrid.RowEventArgs<Avalonia.Controls.Models.TreeDataGrid.HierarchicalRow<TreeNode>> e)
    {
        TreeNode? node = e.Row.Model as TreeNode;
        if (node == null) return;
        node.IsExpanded = false;
    }

    private void NodesSource_RowExpanded(object? sender, Avalonia.Controls.Models.TreeDataGrid.RowEventArgs<Avalonia.Controls.Models.TreeDataGrid.HierarchicalRow<TreeNode>> e)
    {
        TreeNode? node = e.Row.Model as TreeNode;
        if (node == null) return;
        node.IsExpanded = true;
    }

    private void RowSelection_SelectionChanged(object? sender, Avalonia.Controls.Selection.TreeSelectionModelSelectionChangedEventArgs e)
    {
        foreach(var item in e.SelectedItems)
        {
            TreeNode? node = getTreeNode(item);
            if (node == null) continue;
            node.OnSelected();
        }
    }

    private void Tree_DoubleTapped(object? sender, TappedEventArgs e)
    {
        TreeNode? node = getTreeNode(e.Source);
        if (node == null) return;
        node.OnDoubleClicked();
    }

    private void Tree_Tapped(object? sender, TappedEventArgs e)
    {
        TreeNode? node = getTreeNode(e.Source);
        if(node == null) return;
        node.OnClicked();
    }

    public TreeNode? GetSelectedNode()
    {
        if (Tree.RowSelection==null) return null;
        return Tree.RowSelection.SelectedItem as TreeNode;
    }


    private TreeControl2ViewModel viewModel = new TreeControl2ViewModel();
    public TreeNode.TreeNodes Nodes
    {
        get
        {
            return viewModel.Nodes;
        }
    }

    // オブジェクトからTreeNodeを取得する
    private static TreeNode? getTreeNode(object? target)
    {
        if (target == null) return null;
        if (target is TreeNode)
        {
            return target as TreeNode;
        }
        if (target is TextBlock)
        { // targetがTextBockの場合は親objectを探索する
            TextBlock? textBlock = target as TextBlock;
            if (textBlock == null) return null;
            return getTreeNode(textBlock.Parent);
        }else if(target is Image)
        {
            Image? image = target as Image;
            if (image == null) return null;
            return getTreeNode(image.Parent);
        }
        else if (target is StackPanel)
        { // targetがStackPanelの場合は親objectを探索する
            StackPanel? stackPanel = target as StackPanel;
            if (stackPanel == null) return null;
            return getTreeNode(stackPanel.Parent);
        }
        //else if (target is TreeViewItem)
        //{ // targetがTreeViewItemの場合はBindされているTreeNodeを取得する
        //    TreeViewItem? treeViewItem = target as TreeViewItem;
        //    if (treeViewItem == null) return null;
        //    return treeViewItem.DataContext as TreeNode;
        //}
        //else if(target is ScrollContentPresenter)
        //{
        //    ScrollContentPresenter? scp = target as ScrollContentPresenter;
        //    return null;
        //}
        else if (target is Border)
        {
            Border? border = target as Border;
            if (border == null) return null;
            Control? child = border.Child;
            if (child == null) return null;
            return getTreeNode(child.DataContext);
        }
        else if (target is Avalonia.Controls.Presenters.ContentPresenter)
        {
            Avalonia.Controls.Presenters.ContentPresenter? presenter = target as Avalonia.Controls.Presenters.ContentPresenter;
            if (presenter == null) return null;
            if (presenter.Content is TreeNode)
            {
                TreeNode? node = presenter.Content as TreeNode;
                if (node == null) return null;
                return node;
            }
            return null;
        }
        else
        { // ほしいobjectが見つからなかった場合
            return null;
        }
    }

}