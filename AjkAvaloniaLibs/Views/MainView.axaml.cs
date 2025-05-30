﻿using AjkAvaloniaLibs.Controls;
using Avalonia.Controls;
using ExCSS;

namespace AjkAvaloniaLibs.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // ListView initialize
        ListViewItem listItem1 = new ListViewItem("item1");
        ListViewItem listItem2 = new ListViewItem("item2", Avalonia.Media.Colors.Red);
        ListViewItem listItem3 = new ListViewItem("item3", Avalonia.Media.Colors.Blue);

        ListView1.Items.Add(listItem1);
        ListView1.Items.Add(listItem2);
        ListView1.Items.Add(listItem3);



        // Tree1 initialize
        {
            TreeNode node1 = new TreeNode("node1");
            TreeNode node2 = new TreeNode("node2");
            TreeNode node3 = new TreeNode("node3");

            Tree1.Nodes.Add(node1);
            Tree1.Nodes.Add(node2);

            node1.Nodes.Add(node3);

            for (int i = 0; i < 10; i++)
            {
                TreeNode node4 = new TreeNode("node4");
                node3.Nodes.Add(node4);
            }
        }

        // Color Label
        ColorLabel label = new ColorLabel();
        label.AppendText("Block1");
        label.AppendText("Bl\r\nock2",Avalonia.Media.Colors.Red);
        label.AppendText("Block3",Avalonia.Media.Colors.Blue);

        ColorLabel1.Add(label);

        BtnAdd.Tapped += BtnAdd_Tapped;
        BtnRemove.Tapped += BtnRemove_Tapped;
        BtnChange.Tapped += BtnChange_Tapped;

    }

    private void BtnChange_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        Tree1.Nodes[0].Text = "aaa";
    }

    private void BtnRemove_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        Tree1.Nodes[0].Nodes[0].Nodes.RemoveAt(1);
    }

    private void BtnAdd_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        Tree1.Nodes[0].Nodes[0].Nodes.Add(new TreeNode("new_"));
    }
}
