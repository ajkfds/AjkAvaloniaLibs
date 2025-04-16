
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AjkAvaloniaLibs.Controls.TreeNode;

namespace AjkAvaloniaLibs.Controls
{
    public class TreeControl2ViewModel : INotifyPropertyChanged
    {
        public TreeNodes Nodes = new TreeNodes(null);


        public HierarchicalTreeDataGridSource<TreeNode> NodesSource { get; }

        public TreeControl2ViewModel()
        {

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

            NodesSource = new HierarchicalTreeDataGridSource<TreeNode>(Nodes.ReadOnlyNodes)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<TreeNode>(
                        new TemplateColumn<TreeNode>("", CreateCellTemplate()),
                        x => x.Nodes.ReadOnlyNodes
                    )
                },
            };

        }


        private double _fontSize = 14;

        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                RowHeight = _fontSize * 1.2;
                ToggleIconHeight = _fontSize * 0.8;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        private double _rowHeight = 50;
        public double RowHeight
        {
            private set
            {
                if (_rowHeight == value) return;
                _rowHeight = value;
                OnPropertyChanged(nameof(RowHeight));
            }
            get
            {
                return _rowHeight;
            }
        }

        private double _toggleIconHeight = 50;
        public double ToggleIconHeight
        {
            private set
            {
                if (_toggleIconHeight == value) return;
                _toggleIconHeight = value;
                OnPropertyChanged(nameof(ToggleIconHeight));
            }
            get
            {
                return _toggleIconHeight;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static IDataTemplate CreateCellTemplate()
        {
            return new FuncDataTemplate<TreeNode>((node, _) =>
            {
                var image = new Image
                {
                    Stretch = Stretch.Uniform,
                    Source = node.Image
                };

                image.Bind(Image.WidthProperty, new Binding
                {
                    Path = "DataContext.FontSize",
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(TreeDataGrid)
                    }
                });

                image.Bind(Image.HeightProperty, new Binding
                {
                    Path = "DataContext.FontSize",
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(TreeDataGrid)
                    }
                });

                var text = new TextBlock
                {
                    Text = node.Text,
                    Margin = new Thickness(5, 0, 0, 0),
                    MinHeight = 0
                };

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { image, text },
                    VerticalAlignment = VerticalAlignment.Center,
                    MinHeight = 0
                };
            }, true);
        }
    }
}
