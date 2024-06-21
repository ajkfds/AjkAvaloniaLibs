using AjkAvaloniaLibs.Libs;
using Avalonia.Media;
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

namespace AjkAvaloniaLibs.Contorls
{
    public class TreeNode : INotifyPropertyChanged
    {
        public TreeNode()
        {
            Nodes = new TreeNodes(this);
        }

        public TreeNode(string text) : this()
        {
            Text = text;
        }


        // 子ノード
        public TreeNodes Nodes;

        // アイコン画像
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
                NotifyPropertyChanged();
            }
        }

        // ノード展開状態
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
                    OnExpand();
                }
                if (prev & !_IsExpanded)
                {
                    OnCollapse();
                }
            }
        }

        // 親ノード WeakReferenceで保持する
        private System.WeakReference<TreeNode>? parent;
        public TreeNode? Parent
        {
            get
            {
                TreeNode ret;
                if (parent == null) return null;
                if (!parent.TryGetTarget(out ret)) return null;
                return ret;
            }
            set
            {
                if (value == null)
                {
                    parent = null;
                }
                else
                {
                    parent = new WeakReference<TreeNode>(value);
                }
            }
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

        // Viewからの参照のために保持する
        public ReadOnlyObservableCollection<TreeNode> _nodes
        {
            get { return Nodes.ReadOnlyNodes; }
        }

        // 双方向BIndingのためのViewModelへのProperty変更通知
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        // 子ノードを保持するクラス
        // Parenetを保持するためにクラスを実装するs
        public class TreeNodes : IEnumerable<TreeNode>
        {
            private ObservableCollection<TreeNode> nodes;
            private ReadOnlyObservableCollection<TreeNode> rNodes;

            private TreeNode? parent;

            public TreeNodes()
            {
                nodes = new ObservableCollection<TreeNode>();
                rNodes = new ReadOnlyObservableCollection<TreeNode>(nodes);
            }

            public TreeNodes(TreeNode? parent) : this()
            {
                this.parent = parent;
            }


            public ReadOnlyObservableCollection<TreeNode> ReadOnlyNodes
            {
                get { return rNodes; }
            }

            // Parentを保持する
            public void Add(TreeNode treeNode)
            {
                nodes.Add(treeNode);
                treeNode.Parent = parent;
            }

            public void Remove(TreeNode treeNode)
            {
                nodes.Remove(treeNode);
                treeNode.Parent = null;
            }

            public void Insert(int index, TreeNode node)
            {
                nodes.Insert(index, node);
                node.Parent = parent;
            }

            public void Clear()
            {
                nodes.Clear();
            }

            public IEnumerator<TreeNode> GetEnumerator()
            {
                return ((IEnumerable<TreeNode>)nodes).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)nodes).GetEnumerator();
            }
        }

    }
}
