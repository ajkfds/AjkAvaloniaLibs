using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
