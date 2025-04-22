using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AjkAvaloniaLibs.Controls
{
    internal interface ITreeNodeOwner
    {
        ObservableCollection<TreeNode> Nodes { get; }
    }
}
