using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AjkAvaloniaLibs.Controls.TreeControls
{
    /// <summary>
    /// TreeControlのNode Object
    /// Nodes PropertyによってTree状の構造を持ち、TreeのTopはTreeControlとなる。
    /// UI描画、操作は全てTreeControlを介して行う。
    /// </summary>
    /// 
    public class TreeNode : ITreeNodeOwner
    {
        public TreeNode()
        {
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        public TreeNode(string text) : this()
        {
            Text = text;
        }

        internal TreeControl? treeControl { get; set; } = null;

        /// <summary>
        /// Disposes the TreeNode and all its child nodes.
        /// Removes parent references, TreeItem references, and event handlers.
        /// </summary>
        internal void Dispose()
        {
            if (nodes != null)
            {
                foreach (TreeNode childNode in nodes)
                {
                    childNode.Dispose();
                }
            }
        }

        private void UpdateIndentRecursive(TreeNode node)
        {
            node.Indent = Indent + 1;
            foreach (TreeNode child in node.Nodes)
            {
                UpdateIndentRecursive(child);
            }
        }

        private ObservableCollection<TreeNode> nodes = new ObservableCollection<TreeNode>();
        public ObservableCollection<TreeNode> Nodes
        {
            get { return nodes; }
            set
            {
//                nodes.CollectionChanged -= Nodes_CollectionChanged;
                if (treeControl != null)
                {
                    treeControl.AllNodesChanged(this);
                }
                nodes = value;
                nodes.CollectionChanged += Nodes_CollectionChanged;
            }
        }

        internal bool GetNextTo(out TreeNode? nextTo)
        {
            nextTo = null;

            // get owner
            ITreeNodeOwner? owner;
            if (_parent == null) return true;
            if (!_parent.TryGetTarget(out owner)) return false;

            // get subnode lists which this node owner has
            ObservableCollection<TreeNode>? ownerNodes;

            TreeNode? ownerTreeNode = null;
            if (owner is TreeNode) // this is a subnode of a treenode
            {
                ownerNodes = ((TreeNode)owner).Nodes;
                ownerTreeNode = (TreeNode)owner;
            }
            else if (owner is TreeControl) // this is root node
            {
                ownerNodes = ((TreeControl)owner).Nodes;
            }
            else
            {
                System.Diagnostics.Debugger.Break();
                return false;
            }

            int index = ownerNodes.IndexOf(this);
            if (index < 0)
            {   // lost owner
                return false;
            }

            if (index == 0) // top item of owner nodes
            {
                if (ownerTreeNode == null) return true;
                else
                {
                    nextTo = ownerTreeNode;
                    return true;
                }
            }

            TreeNode previousNode = ownerNodes[index - 1];
            if (previousNode.IsExpanded && previousNode.Nodes.Count != 0)
            {
                previousNode = previousNode.Nodes.Last<TreeNode>();
            }
            nextTo = previousNode;
            return true;
        }

        private IImage? bitmap = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/paper.svg");
        public IImage? Image
        {
            get
            {
                return bitmap;
            }
            set
            {
                bitmap = value;
                if (TreeItem != null) TreeItem.updateVisual();
            }
        }

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                bool prev = _IsExpanded;
                _IsExpanded = value;
                UpdateIndent(this);
                if (!prev & _IsExpanded)
                {
                    if (treeControl != null) treeControl.NodeExpanded(this);
                    OnExpand();
                }
                if (prev & !_IsExpanded)
                {
                    if (treeControl != null) treeControl.NodeCollapsed(this);
                    OnCollapse();
                }
            }
        }

        private bool _selected = false;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (!Dispatcher.UIThread.CheckAccess() && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                _selected = value;
                if (TreeItem != null) TreeItem.updateVisual();
            }
        }

        // 親ノード WeakReferenceで保持する
        internal System.WeakReference<ITreeNodeOwner>? _parent = null;
        internal ITreeNodeOwner? parent
        {
            get
            {
                ITreeNodeOwner? ret;
                if (_parent == null) return null;
                if (!_parent.TryGetTarget(out ret)) return null;
                return ret;
            }
            set
            {
                if (value == null)
                {
                    _parent = null;
                }
                else
                {
                    _parent = new WeakReference<ITreeNodeOwner>(value);
                }
            }
        }

        public ITreeNodeOwner? Parent
        {
            get
            {
                return parent as TreeNode;
            }
        }


        internal System.WeakReference<TreeControlViewItem>? _treeItem = null;
        internal TreeControlViewItem? TreeItem
        {
            get
            {
                TreeControlViewItem? ret;
                if (_treeItem == null) return null;
                if (!_treeItem.TryGetTarget(out ret)) return null;
                return ret as TreeControlViewItem;
            }
            set
            {

                if (value == null)
                {
                    _treeItem = null;
                }
                else
                {
                    _treeItem = new WeakReference<TreeControlViewItem>(value);
                }
            }
        }


        private void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }

            if (treeControl != null) treeControl.Nodes_CollectionChanged(this, e);
        }

 

        /// <summary>
        /// update node indent value using parent indent
        /// </summary>
        /// <param name="ownerNode"></param>
        internal void UpdateIndent(ITreeNodeOwner ownerNode)
        {
            foreach (TreeNode node in ownerNode.Nodes)
            {
                node.parent = ownerNode;
                if (ownerNode.Indent + 1 != node.Indent)
                {
                    node.Indent = ownerNode.Indent + 1;
                    UpdateIndent(node);
                }
            }
        }

        public int Indent { get; set; } = 0;
        public bool Visible { get; set; } = false;
        
        // Virtual interface -----------------------------------

        // ノード展開時に呼ばれる
        public virtual void OnExpand() { }

        // ノードを閉じたときに呼ばれる
        public virtual void OnCollapse() { }

        // ノードが選択されたときに呼ばれる
        public virtual void OnSelected() { }


        public virtual void OnDeSelected() { }

        // ノードがクリックされたときに呼ばれる
        public virtual void OnClicked() { }

        // ノードがダブルクリックされたときに呼ばれる
        public virtual void OnDoubleClicked() { }

        // ノードテキスト
        private string _Text = "";
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

    }
}
