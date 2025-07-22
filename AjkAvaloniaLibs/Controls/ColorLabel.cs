using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ExCSS;
using HarfBuzzSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AjkAvaloniaLibs.Controls
{
    public class ColorLabel : IDisposable
    {
        public ColorLabel()
        {

        }
        public void Dispose()
        {
            items.Clear();
            items = null;
        }

        public List<labelItem> GetItems()
        {
            return items;
        }

        public int ItemCount
        {
            get
            {
                return items.Count;
            }
        }

        private List<labelItem> items = new List<labelItem>();

        public interface labelItem
        {
        }
        public class labelText : labelItem
        {
            public labelText(string line)
            {
                Text = line;
            }
            public labelText(string line, Avalonia.Media.Color color)
            {
                Text = line;
                Color = color;
            }
            public string Text;
            public Avalonia.Media.Color? Color = null;

            //public Size GetSize(Graphics graphics, Font font)
            //{
            //    return System.Windows.Forms.TextRenderer.MeasureText(graphics, Text, font, new Size(10, 10), System.Windows.Forms.TextFormatFlags.NoPadding);
            //}
            //public void Draw(Graphics graphics, int x, int y, Font font, Color DefaultColor, Color bgColor)
            //{
            //    Color color = DefaultColor;
            //    if (this.Color != null) color = (Color)this.Color;
            //    System.Windows.Forms.TextRenderer.DrawText(
            //        graphics,
            //        Text,
            //        font,
            //        new Point(x, y),
            //        color,
            //        bgColor,
            //        System.Windows.Forms.TextFormatFlags.NoPadding
            //        );
            //}
        }

        public class labelImage : labelItem
        {
            public labelImage(IImage image)
            {
                   this.Image = image;
            }
            public IImage Image;
            public bool IconSize = false;
        }
        //public class labelIconImage : labelItem
        //{
        //    public labelIconImage(Primitive.IconImage iconImage, Primitive.IconImage.ColorStyle colorStyle)
        //    {
        //        this.IconImage = iconImage;
        //        this.ColorStyle = colorStyle;
        //    }
        //    public Primitive.IconImage.ColorStyle ColorStyle;

        //    public Size GetSize(int height)
        //    {
        //        return new Size(IconImage.GetImageWidth(height), height);
        //    }
        //    public void Draw(Graphics graphics, int x, int y, int height)
        //    {
        //        graphics.DrawImage(this.IconImage.GetImage(height, ColorStyle), new Point(x, y));
        //    }
        //}
        public class labelNewLine : labelItem
        {
        }


        /*
        public override void Draw(Graphics graphics, int x, int y, Font font, Color backgroundColor)
        {
            Size tsize = System.Windows.Forms.TextRenderer.MeasureText(graphics, text, font);
            if (icon != null) graphics.DrawImage(icon.GetImage(tsize.Height, iconColorStyle), new Point(x, y));
            Color bgColor = backgroundColor;
            System.Windows.Forms.TextRenderer.DrawText(
                graphics,
                text,
                font,
                new Point(x + tsize.Height + (tsize.Height >> 2), y),
                color,
                bgColor,
                System.Windows.Forms.TextFormatFlags.NoPadding
                );
        }
        */


        public void AppendLabel(ColorLabel label)
        {
            if (label == null)
            {
                //               System.Diagnostics.Debugger.Break();
                return;
            }
            List<labelItem> newItems = label.GetItems();
            foreach (labelItem item in newItems)
            {
                items.Add(item);
            }
        }

        public void Clear()
        {
            items.Clear();
        }

        public void AppendText(string text)
        {
            string linesStr = text.Replace("\r\n", "\n");
            items.Add(new labelText(linesStr));
        }

        public void AppendText(string text, Avalonia.Media.Color color)
        {
            string linesStr = text.Replace("\r\n", "\n");
            items.Add(new labelText(linesStr, color));
        }

        public void AppendImage(IImage image)
        {
            items.Add(new labelImage(image));
        }

        public void AppendIconImage(IImage image)
        {
            labelImage labelImage = new labelImage(image);
            labelImage.IconSize = true;
            items.Add(labelImage);
        }
        
        //public void Draw(Graphics graphics, int x, int y, Font font, Color defaultColor, Color backgroundColor)
        //{
        //    Size size;
        //    drawLabels(graphics, x, y, font, defaultColor, backgroundColor, out size, true);
        //}

        //public Size GetSize(Graphics graphics, Font font)
        //{
        //    Size size;
        //    drawLabels(graphics, 0, 0, font, Color.Black, Color.Black, out size, false);
        //    return size;
        //}

        public void RemoveLastNewLine()
        {
            if (items.Count < 1) return;
            if (items.Last() is labelNewLine)
            {
                items.RemoveAt(items.Count - 1);
            } else if(items.Last() is labelText)
            {
                labelText labelText = (labelText)items.Last();
                if (!labelText.Text.EndsWith("\n")) return;
                labelText.Text = labelText.Text.Substring(0, labelText.Text.Length - 1);
                if(labelText.Text.Length==0) items.RemoveAt(items.Count - 1);
            }
        }

        public void AppendToTextBlock(TextBlock textBlock)
        {
            if (textBlock.Inlines == null) return;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is labelNewLine)
                {
                    textBlock.Inlines.Add(new Run("\n"));
                }
                else if (items[i] is labelText)
                {
                    labelText textItem = (labelText)items[i];
                    Run run = new Run(textItem.Text);
                    if (textItem.Color != null) {
                        run.Foreground = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)textItem.Color);
                    }
                    textBlock.Inlines.Add(run);
                }
                else if (items[i] is labelImage)
                {
                    labelImage labelImage = (labelImage)items[i];

                    Avalonia.Controls.Image image = new Avalonia.Controls.Image();
                    image.Source = labelImage.Image;// new Bitmap(AssetLoader.Open(new Uri("avares://TestApp/Assets/avalonia-logo.ico")));
                    if (labelImage.IconSize)
                    {
                        image.Width = textBlock.FontSize;
                        image.Height = textBlock.FontSize;
                        image.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                    }
                    else
                    {
                        image.Width = labelImage.Image.Size.Width;
                        image.Height = labelImage.Image.Size.Height;
                        image.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                    }


                    InlineUIContainer inlineUIContainer = new InlineUIContainer();
                    inlineUIContainer.BaselineAlignment = BaselineAlignment.Baseline;
                    inlineUIContainer.Child = image;

                    textBlock.Inlines.Add(inlineUIContainer);
                }
                else
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

        }

        public string CreateString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is labelNewLine)
                {
                    sb.Append("\r\n");
                }
                else if (items[i] is labelText)
                {
                    labelText textItem = (labelText)items[i];
                    sb.Append(textItem.Text);
                }
                else if (items[i] is labelImage)
                {
                }
                else
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            return sb.ToString();
        }
    }
}
