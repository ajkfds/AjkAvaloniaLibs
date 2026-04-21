using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Controls
{
    public class TreeGridNode
    {
        public string Name { get; set; }
        public ObservableCollection<TreeGridNode> SubCategories { get; set; }

        public TreeGridNode(string name)
        {
            Name = name;
            SubCategories = new ObservableCollection<TreeGridNode>();
        }
    }
}
