using AjkAvaloniaLibs.Contorls;
using Avalonia.Controls;

namespace AjkAvaloniaLibs.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        TreeNode node1 = new TreeNode("node1");
        TreeNode node2 = new TreeNode("node2");
        TreeNode node3 = new TreeNode("node3");
        TreeNode node4 = new TreeNode("node4");

        Tree1.Nodes.Add(node1);
        Tree1.Nodes.Add(node2);

        node1.Nodes.Add(node3);
        node3.Nodes.Add(node4);

    }
}
