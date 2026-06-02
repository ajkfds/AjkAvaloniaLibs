using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Controls.TreeControls
{
    public interface ITreeNodeOwner
    {
        public ObservableCollection<TreeNode> Nodes { get; }

        public bool Visible { get; }

        public bool IsExpanded { get; }
        public int Indent { get; set; }
    }
}
