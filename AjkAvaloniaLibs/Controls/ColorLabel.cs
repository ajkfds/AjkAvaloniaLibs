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
        //public class labelIconImage : labelItem
        //{
        //    public labelIconImage(Primitive.IconImage iconImage, Primitive.IconImage.ColorStyle colorStyle)
        //    {
        //        this.IconImage = iconImage;
        //        this.ColorStyle = colorStyle;
        //    }
        //    public Primitive.IconImage IconImage;
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
            items.Add(new labelText(text));

            //if (linesStr == "\n")
            //{
            //    items.Add(new labelNewLine());
            //    return;
            //}
            //string[] lines = linesStr.Split('\n');
            //foreach (string line in lines)
            //{
            //    items.Add(new labelText(line));
            //    items.Add(new labelNewLine());
            //}
            //if (!text.EndsWith("\n") && lines.Length != 0) items.Remove(items.Last());
        }

        public void AppendText(string text, Avalonia.Media.Color color)
        {
            string linesStr = text.Replace("\r\n", "\n");
            items.Add(new labelText(text,color));
            //if (linesStr == "\n")
            //{
            //    items.Add(new labelNewLine());
            //    return;
            //}
            //string[] lines = linesStr.Split('\n');
            //foreach (string line in lines)
            //{
            //    items.Add(new labelText(line, color));
            //    items.Add(new labelNewLine());
            //}
            //if (!text.EndsWith("\n") && lines.Length != 0) items.Remove(items.Last());
        }

        //public void AppendIconImage(Primitive.IconImage iconImage, Primitive.IconImage.ColorStyle colorStyle)
        //{
        //    items.Add(new labelIconImage(iconImage, colorStyle));
        //}

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

        //private void drawLabels(Graphics graphics, int x, int y, Font font, Color defaultColor, Color backgroundColor, out Size size, bool draw)
        //{
        //    int dx = x;
        //    int dy = y;
        //    int xSize = 0;
        //    int ySize = 0;
        //    Size tSize;
        //    int lineHeight = System.Windows.Forms.TextRenderer.MeasureText(graphics, "AA", font).Height;

        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        if (items[i] is labelNewLine)
        //        {
        //            dy += lineHeight;
        //            dx = x;
        //        }
        //        else if (items[i] is labelText)
        //        {
        //            labelText textItem = items[i] as labelText;
        //            if (draw) textItem.Draw(graphics, dx, dy, font, defaultColor, backgroundColor);
        //            tSize = textItem.GetSize(graphics, font);
        //            dx += tSize.Width;
        //        }
        //        else if (items[i] is labelIconImage)
        //        {
        //            labelIconImage labelImage = items[i] as labelIconImage;
        //            if (draw) labelImage.Draw(graphics, dx, dy, lineHeight);
        //            tSize = labelImage.GetSize(lineHeight);
        //            dx += tSize.Width;
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debugger.Break();
        //        }
        //        if (dx > xSize) xSize = dx;
        //        if (dy > ySize) ySize = dy;
        //    }
        //    if (items.Count != 0)
        //    {
        //        if (items[items.Count - 1] is labelNewLine)
        //        {

        //        }
        //        else
        //        {
        //            ySize += lineHeight;
        //        }
        //    }

        //    size = new Size(xSize, ySize);
        //}

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
                    labelText textItem = items[i] as labelText;
                    sb.Append(textItem.Text);
                }
                //else if (items[i] is labelIconImage)
                //{
                //}
                else
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            return sb.ToString();
        }
    }
}
