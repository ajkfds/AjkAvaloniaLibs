using Avalonia.Controls;
using Avalonia.Controls.Generators;

namespace AjkAvaloniaLibs.Contorls
{
    public partial class TreeControl : UserControl
    {
        
        public TreeControl()
        {
            InitializeComponent();
            // DataContext��ViewModel�̐ݒ�
            TreeView.DataContext = new TreeControlViewModel();

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

                node1.Nodes.Add(node4);
                node1.Nodes.Add(node5);

                node2.Nodes.Add(node6);
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

        // �N���b�N�n���h��
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


        // �I���A�C�e���ύX�n���h��
        private void TreeView_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            TreeNode? node = getTreeNode(TreeView.SelectedItem);
            if (node == null) return;
            node.OnSelected();
        }

        // �_�u���N���b�N �C�x���g�n���h��
        private void TreeView_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            TreeNode? node = getTreeNode(e.Source);
            if (node == null) return;
            node.OnDoubleClicked();
        }

        // �I�u�W�F�N�g����TreeNode���擾����
        private TreeNode? getTreeNode(object? target)
        {
            if (target == null) return null;
            if (target is TreeNode)
            {
                return target as TreeNode;
            }
            if (target is TextBlock)
            { // target��TextBock�̏ꍇ�͐eobject��T������
                TextBlock? textBlock = target as TextBlock;
                if (textBlock == null) return null;
                return getTreeNode(textBlock.Parent);
            }
            else if (target is StackPanel)
            { // target��StackPanel�̏ꍇ�͐eobject��T������
                StackPanel? stackPanel = target as StackPanel;
                if (stackPanel == null) return null;
                return getTreeNode(stackPanel.Parent);
            }
            else if (target is TreeViewItem)
            { // target��TreeViewItem�̏ꍇ��Bind����Ă���TreeNode���擾����
                TreeViewItem? treeViewItem = target as TreeViewItem;
                if (treeViewItem == null) return null;
                return treeViewItem.DataContext as TreeNode;
            }
            else
            { // �ق���object��������Ȃ������ꍇ
                return null;
            }
        }

    }
}
