using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;

namespace AjkAvaloniaLibs.Controls;

public partial class GridControl : UserControl
{

    public GridControl()
    {
        InitializeComponent();
        DataContext = new FlatGridViewModel();

        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != Avalonia.Input.KeyModifiers.Control) return;
            if (i.Delta.Y > 0) FontSize++;
            else FontSize = FontSize > 1 ? FontSize - 1 : 1;
            FontSize = FontSize;
        }, Avalonia.Interactivity.RoutingStrategies.Bubble, true);

    }
}

