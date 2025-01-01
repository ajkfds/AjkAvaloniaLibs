using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace AjkAvaloniaLibs.Controls
{
    public partial class TreeControl : UserControl
    {
        
        public TreeControl()
        {
            InitializeComponent();
            // DataContextのViewModelの設定
            TreeView.DataContext = new TreeControlViewModel();
            TreeView.AutoScrollToSelectedItem = false;

            if (Design.IsDesignMode)
            {
                TreeNode node1 = new TreeNode("TestNode1");
                TreeNode node2 = new TreeNode("TestNode2");
                TreeNode node3 = new TreeNode("TestNode3");
                TreeNode node4 = new TreeNode("TestNode4");
                TreeNode node5 = new TreeNode("TestNode5");
                TreeNode node6 = new TreeNode("TestNode5");

                Nodes.Add(node1);
                Nodes.Add(node2);
                Nodes.Add(node3);

                //for(int i = 0; i < 100; i++)
                //{
                //    TreeNode node = new TreeNode("TestNode_______________________________________"+i.ToString());
                //    Nodes.Add(node);
                //}

                node1.Nodes.Add(node4);
                node1.Nodes.Add(node5);

                node2.Nodes.Add(node6);
            }

            ScrollViewer? scrollViewer = TreeView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if(scrollViewer != null)
            {
            //    scrollViewer.set = new Avalonia.Size(10, 10);
            }
        }

        public TreeNode.TreeNodes Nodes
        {
            get
            {
                TreeControlViewModel? viewModel = TreeView.DataContext as TreeControlViewModel;
                if (viewModel == null) throw new System.Exception();
                return viewModel.Nodes;
            }
        }

        public TreeNode? GetSelectedNode()
        {
            TreeNode? selected = TreeView.SelectedItem as TreeNode;
            return selected;
        }

        // クリックハンドラ
        public Action<TreeNode> NodeClicked;
        private void TreeView_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TreeNode? node = getTreeNode(e.Source);
            if (node == null)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
            if (node == null) return;
            if (NodeClicked != null) NodeClicked(node);
            node.OnClicked();
        }


        // 選択アイテム変更ハンドラ
        private void TreeView_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            TreeNode? node = getTreeNode(TreeView.SelectedItem);
            if (node == null) return;
            node.OnSelected();
        }

        // ダブルクリック イベントハンドラ
        private void TreeView_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TreeNode? node = getTreeNode(e.Source);
            if (node == null) return;
            if (node._nodes.Count > 0)
            {
                node.IsExpanded = !node.IsExpanded;
            }
            node.OnDoubleClicked();
        }

        // オブジェクトからTreeNodeを取得する
        private TreeNode? getTreeNode(object? target)
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
            }
            else if (target is StackPanel)
            { // targetがStackPanelの場合は親objectを探索する
                StackPanel? stackPanel = target as StackPanel;
                if (stackPanel == null) return null;
                return getTreeNode(stackPanel.Parent);
            }
            else if (target is TreeViewItem)
            { // targetがTreeViewItemの場合はBindされているTreeNodeを取得する
                TreeViewItem? treeViewItem = target as TreeViewItem;
                if (treeViewItem == null) return null;
                return treeViewItem.DataContext as TreeNode;
            }
            else if(target is Border)
            {
                Border? border = target as Border;
                if (border == null) return null;
                Control? child = border.Child;
                if (child == null) return null;
                return getTreeNode(child.DataContext);
            }
            else if(target is Avalonia.Controls.Presenters.ContentPresenter)
            {
                Avalonia.Controls.Presenters.ContentPresenter? presenter = target as Avalonia.Controls.Presenters.ContentPresenter;
                if (presenter == null) return null;
                if(presenter.Content is TreeNode)
                {
                    TreeNode? node = presenter.Content as TreeNode;
                    if(node == null) return null;
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
}
