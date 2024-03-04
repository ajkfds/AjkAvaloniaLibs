using Avalonia.Controls;
using Avalonia.Controls.Generators;

namespace AjkAvaloniaLibs.Contorls
{
    public partial class TreeControl : UserControl
    {
        
        public TreeControl()
        {
            InitializeComponent();
            // DataContextのViewModelの設定
            TreeView.DataContext = new TreeControlViewModel();
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

        // クリックハンドラ
        private void TreeView_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TreeNode? node = getTreeNode(e.Source);
            if (node == null)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
            if (node == null) return;
            node.OnClicked();
        }

        private void TreeView_Tapped_1(object? sender, Avalonia.Input.TappedEventArgs e)
        {
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
            else
            { // ほしいobjectが見つからなかった場合
                return null;
            }
        }

    }
}
