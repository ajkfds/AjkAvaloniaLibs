using AjkAvaloniaLibs.Libs;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
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
                ObservableCollection<TreeNode>? ownerNodes;
                TreeNode? owner = null;
                ITreeNodeOwner? ret;
                if (_parent == null) return null;
                if (!_parent.TryGetTarget(out ret)) return null;

                if (ret is TreeNode)
                {
                    ownerNodes = ((TreeNode)ret).Nodes;
                    owner = (TreeNode)ret;
                }
                else if (ret is TreeControl)
                {
                    ownerNodes = ((TreeControl)ret).Nodes;
                }
                else
                {
                    return null;
                }

                int index = ownerNodes.IndexOf(this);
                if(index < 0) return null;
                if (index == 0)
                {
                    if (owner == null) return null;
                    else return owner;
                }
                return ownerNodes[index - 1];
            }
        }

        private IImage? bitmap = AjkAvaloniaLibs.Libs.Icons.GetSvgBitmap("AjkAvaloniaLibs/Assets/Icons/paper.svg");
        public virtual IImage? Image
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
            set {
                _selected = value;
                if(TreeItem != null) TreeItem.updateVisual();
            } 
        }
        internal Action<TreeNode>? ReportExpanded { get; set; } = null;
        internal Action<TreeNode>? ReportCollapsed { get; set; } = null;

        internal bool Visible = false;

        // 親ノード WeakReferenceで保持する
        internal System.WeakReference<ITreeNodeOwner>? _parent = null;
        public TreeNode? Parent
        {
            get
            {
                ITreeNodeOwner? ret;
                if (_parent == null) return null;
                if (!_parent.TryGetTarget(out ret)) return null;
                return ret as TreeNode;
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

        internal System.WeakReference<TreeControl.TreeItem>? _treeItem = null;
        internal TreeControl.TreeItem? TreeItem
        {
            get
            {
                TreeControl.TreeItem? ret;
                if (_treeItem == null) return null;
                if (!_treeItem.TryGetTarget(out ret)) return null;
                return ret as TreeControl.TreeItem;
            }
            set
            {
                if (value == null)
                {
                    _treeItem = null;
                }
                else
                {
                    _treeItem = new WeakReference<TreeControl.TreeItem>(value);
                }
            }
        }


        public Action<object?, System.Collections.Specialized.NotifyCollectionChangedEventArgs>? CollectionChanged { get; set; } = null;
        private void Nodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TreeNode node in e.NewItems)
                {
                    node.Parent = this;
                    if (node.Parent == null)
                    {
                        node.Indent = 0;
                    }
                    else
                    {
                        node.Indent = Indent + 1;
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (TreeNode node in e.OldItems)
                {
                    node.Parent = null;
                    node.Indent = 0;
                }
            }
            // raise upper layer
            if (TreeItem != null) TreeItem.updateVisual();
            if (PropageteCollectionChange != null) PropageteCollectionChange(this, e);
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

        // ノードがクリックされたときに呼ばれる
        public virtual void OnClicked() { }

        // ノードがダブルクリックされたときに呼ばれる
        public virtual void OnDoubleClicked() { }

        // ノードテキスト
        private string _Text = "";
        public virtual string Text
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
