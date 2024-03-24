using Avalonia.Controls;
using Avalonia.Media;
using DynamicData;
using System.Reflection.Emit;

namespace AjkAvaloniaLibs.Contorls
{
    public partial class ColorLabelControl : UserControl
    {
        public ColorLabelControl()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            TextBlock1.Inlines.Clear();
        }

        public void Add(ColorLabel colorLabel)
        {
            foreach(var item in colorLabel.GetItems())
            {
                if(item is ColorLabel.labelText)
                {
                    ColorLabel.labelText label = item as ColorLabel.labelText;
                    Avalonia.Controls.Documents.Run run = new Avalonia.Controls.Documents.Run(label.Text);
                    if(label.Color != null)
                    {
                        run.Foreground = new SolidColorBrush((Avalonia.Media.Color)label.Color);
                    }
                    TextBlock1.Inlines.Add(run);
                }
            }

        }


    }
}
