using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AjkAvaloniaLibs.Controls
{
    public class TreeNode : INotifyPropertyChanged, ITreeNodeOwner
    {
        public TreeNode()
        {
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        public TreeNode(string text) : this()
        {
            Text = text;
        }

        internal void RemoveFromPropagateTree()
        {
            CollectionChanged = null;
            _parent = null;
            PropageteCollectionChange = null;
            ReportExpanded = null;
            ReportCollapsed = null;
        }

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
            
            parent = null;
            TreeItem = null;
            PropageteCollectionChange = null;
            ReportExpanded = null;
            ReportCollapsed = null;
            CollectionChanged = null;
            PropertyChanged = null;
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
                // Store the old collection reference before replacing
                ObservableCollection<TreeNode>? oldNodes = nodes;

                nodes = value;
                if (nodes != null)
                {
                    nodes.CollectionChanged += Nodes_CollectionChanged;

                    // Re-parent existing nodes in new collection
                    foreach (TreeNode newNode in nodes)
                    {
                        newNode.parent = this;
                        newNode.PropageteCollectionChange += Nodes_CollectionChangeInform;
                        newNode.PropertyChanged += Node_PropertyChanged;
                        UpdateIndentRecursive(newNode);
                    }
                }

                // Raise reset notification to update TreeControl.Items
                // This must happen BEFORE removing from propagation tree, so PropageteCollectionChange is still valid
                OnCollectionChanged(this,
                    new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                        System.Collections.Specialized.NotifyCollectionChangedAction.Reset));

                // Now remove old nodes from propagation tree and dispose them
                // TreeControl has already processed the Reset, so TreeItem references are no longer needed
                if (oldNodes != null)
                {
                    foreach (TreeNode oldNode in oldNodes)
                    {
                        oldNode.RemoveFromPropagateTree();
                        oldNode.Dispose();
                    }
                }
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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
                if (!prev & _IsExpanded)
                {
                    if (ReportExpanded != null) ReportExpanded(this);
                    OnExpand();
                }
                if (prev & !_IsExpanded)
                {
                    if (ReportCollapsed != null) ReportCollapsed(this);
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
//                if (_selected == value) return;
                _selected = value;
                if (TreeItem != null) TreeItem.updateVisual();
                NotifyPropertyChanged(nameof(Selected));
            }
        }
        internal Action<TreeNode>? ReportExpanded { get; set; } = null;
        internal Action<TreeNode>? ReportCollapsed { get; set; } = null;

        internal bool Visible = false;

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

        public TreeNode? Parent
        {
            get
            {
                return parent as TreeNode;
            }
        }


        internal System.WeakReference<TreeControl.TreeViewItem>? _treeItem = null;
        internal TreeControl.TreeViewItem? TreeItem
        {
            get
            {
                TreeControl.TreeViewItem? ret;
                if (_treeItem == null) return null;
                if (!_treeItem.TryGetTarget(out ret)) return null;
                return ret as TreeControl.TreeViewItem;
            }
            set
            {
                if (value == null)
                {
                    _treeItem = null;
                }
                else
                {
                    _treeItem = new WeakReference<TreeControl.TreeViewItem>(value);
                }
            }
        }


        public Action<object?, System.Collections.Specialized.NotifyCollectionChangedEventArgs>? CollectionChanged { get; set; } = null;
        private void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                OnCollectionChanged(sender, e);
            }
            else
            {
                Dispatcher.UIThread.Post(() => OnCollectionChanged(sender, e));
            }
        }

        private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.UIThread.CheckAccess()) throw new Exception();


            UpdateIndent(this);

            if (e.OldItems != null)
            {
                foreach (TreeNode node in e.OldItems)
                {
                    node.PropageteCollectionChange -= Nodes_CollectionChangeInform;
                    node.PropertyChanged -= Node_PropertyChanged;
                }
            }

            // Batch update all visible TreeViewItems under the Parent
            BatchUpdateParentVisual();

            // raise upper layer
            if (PropageteCollectionChange != null)
            {
                PropageteCollectionChange(this, e);
            }
        }

        /// <summary>
        /// Batch update all visible TreeViewItems under the Parent node.
        /// Called when collection changes occur to update all affected nodes at once.
        /// </summary>
        private void BatchUpdateParentVisual()
        {
            // Get the TreeControl that owns this node's hierarchy
            ITreeNodeOwner? owner = parent;
            if (owner == null) return;

            // Find the root TreeControl
            TreeControl? treeControl = null;
            if (owner is TreeControl tc)
            {
                treeControl = tc;
            }
            else if (owner is TreeNode ownerNode)
            {
                // Traverse up to find the TreeControl
                ITreeNodeOwner? current = owner;
                while (current != null)
                {
                    if (current is TreeControl rootTc)
                    {
                        treeControl = rootTc;
                        break;
                    }
                    if (current is TreeNode tn)
                    {
                        current = tn.parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (treeControl == null) return;

            // Batch update all TreeViewItems
            treeControl.BatchUpdateVisual();
        }

        internal void UpdateIndent(TreeNode ownerNode)
        {
            foreach (TreeNode node in ownerNode.Nodes)
            {
                node.parent = this;
                node.PropageteCollectionChange = Nodes_CollectionChangeInform;
                node.PropertyChanged = Node_PropertyChanged;
                if (ownerNode.Indent + 1 != node.Indent)
                {
                    node.Indent = ownerNode.Indent + 1;
                    UpdateIndent(node);
                }
            }
        }

        private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (Parent == null || Parent.PropertyChanged == null) return;
            Parent.PropertyChanged(sender, new PropertyChangedEventArgs(e.PropertyName));
        }

        public Action<TreeNode, System.Collections.Specialized.NotifyCollectionChangedEventArgs>? PropageteCollectionChange { get; set; } = null;
        private void Nodes_CollectionChangeInform(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (PropageteCollectionChange != null) PropageteCollectionChange(this, e);
        }


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
            set { _Text = value; NotifyPropertyChanged(); }
        }
        public int Indent { get; set; } = 0;

        // 双方向BIndingのためのViewModelへのProperty変更通知
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
