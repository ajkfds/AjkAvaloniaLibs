using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Controls
{
    internal interface ITreeNodeOwner
    {
        ObservableCollection<TreeNode> Nodes { get; }
    }
}
