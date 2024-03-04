using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static AjkAvaloniaLibs.Contorls.TreeNode;

namespace AjkAvaloniaLibs.Contorls
{
    public class TreeControlViewModel
    {
        public TreeControlViewModel()
        {
            Nodes = new TreeNodes(null);
        }

        // 子ノード
        public TreeNodes Nodes;

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

    }
}
