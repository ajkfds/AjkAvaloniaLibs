using AjkAvaloniaLibs.Libs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using ExCSS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AjkAvaloniaLibs.Controls
{
    public class TreeNode : INotifyPropertyChanged,ITreeNodeOwner
    {
        public TreeNode()
        {
            Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        public TreeNode(string text) : this()
        {
            Text = text;
        }

        public ObservableCollection<TreeNode> Nodes { get; } = new ObservableCollection<TreeNode>();
        internal TreeNode? NextTo {
            get {
                // get owner
                ITreeNodeOwner? owner;
                if (_parent == null) return null;
                if (!_parent.TryGetTarget(out owner)) return null;

                // get owenerNodes,ownerTreeNode
                ObservableCollection<TreeNode>? ownerNodes;

                TreeNode? ownerTreeNode = null;
                if (owner is TreeNode)
                {
                    ownerNodes = ((TreeNode)owner).Nodes;
                    ownerTreeNode = (TreeNode)owner;
                }
                else if (owner is TreeControl)
                {
                    ownerNodes = ((TreeControl)owner).Nodes;
                }
                else
                {
                    return null;
                }

                int index = ownerNodes.IndexOf(this);
                if (index < 0) return null;

                if (index == 0) // top item of owner nodes
                {
                    if (ownerTreeNode == null) return null;
                    else return ownerTreeNode;
                }

                TreeNode previousNode = ownerNodes[index - 1];
                while (previousNode.IsExpanded)
                {
                    previousNode = previousNode.Nodes.Last<TreeNode>();
                }
                return previousNode;
            }
        }

        private IImage? bitmap = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/paper.svg");
        public  IImage? Image
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
                updateIndent();
                NotifyPropertyChanged();
                if (!prev & _IsExpanded)
                {
                    if(ReportExpanded != null) ReportExpanded(this);
                    OnExpand();
                }
                if (prev & !_IsExpanded)
                {
                    if(ReportCollapsed != null) ReportCollapsed(this);
                    OnCollapse();
                }
            }
        }

        private bool _selected = false;
        public bool Selected {
            get { return _selected; } 
            internal set {
                _selected = value;
                if(TreeItem != null) TreeItem.updateVisual();
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
            if (!Dispatcher.UIThread.CheckAccess()) throw new Exception();
            if (e.OldItems != null)
            {
                foreach (TreeNode node in e.OldItems)
                {
                    node.parent = null;
                    node.Indent = 0;
                    node.PropageteCollectionChange -= Nodes_CollectionChangeInform;
                    node.PropertyChanged -= Node_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (TreeNode node in e.NewItems)
                {
                    System.Diagnostics.Debug.Print("## node add "+node.Text );
                    node.parent = this;
                    node.updateIndent();
                    node.PropageteCollectionChange += Nodes_CollectionChangeInform;
                    node.PropertyChanged += Node_PropertyChanged;
                }
            }
            // raise upper layer
            if (TreeItem != null) TreeItem.updateVisual();
            if (PropageteCollectionChange != null)
            {
                PropageteCollectionChange(this, e);
            }
        }

        internal void updateIndent()
        {
            if (Parent != null)
            {
                Indent = Parent.Indent+1;
            }
            else
            {
                Indent = 0;
            }

            foreach (TreeNode node in Nodes)
            {
                node.parent = this;
                node.updateIndent();
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
