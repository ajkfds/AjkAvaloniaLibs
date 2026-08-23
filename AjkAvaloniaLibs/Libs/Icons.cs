using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using ExCSS;
// SKPictureに対する拡張メソッドとしてSKPicture.ToImageが定義されている。これを使うのに必要。
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;

namespace AjkAvaloniaLibs.Libs
{
    public static class Icons
    {
        public static Dictionary<string, Bitmap> iconImages = new Dictionary<string, Bitmap>();

        //public enum ColorStyle : int
        //{
        //    Original = 0,
        //    Gray = 1,
        //    Red = 2,
        //    Blue = 3,
        //    Green = 4,
        //    White = 5,
        //    Orange = 6,
        //}

        /// <summary>
        /// Assets/Icons/以下の(iconName).svgをIImage形式にして取り出す。
        /// 一度使ったIImageはキャッシュする。
        /// </summary>
        public static Bitmap GetSvgBitmap(string SvgPath)
        {
            string iconName = SvgPath.Substring(SvgPath.LastIndexOf('/') + 1);
            if (!SvgPath.ToLower().EndsWith(".svg")) throw new Exception();
            iconName = iconName.Substring(0, iconName.Length - 4);

            if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

            SKBitmap skBitmap = getSkBitmapFromSvg(SvgPath, 1f, 1f);
            Bitmap bmp = getBitmapFromSKBitmap(skBitmap);

            lock (iconImages)
            {
                iconImages.Add(iconName, bmp);
            }

            return iconImages[iconName];
        }

        public static Bitmap GetSvgBitmap(string SvgPath, Avalonia.Media.Color color)
        {
            string iconName = SvgPath.Substring(SvgPath.LastIndexOf('/') + 1);
            if (!SvgPath.ToLower().EndsWith(".svg")) throw new Exception();
            iconName = iconName.Substring(0, iconName.Length - 4);
            iconName += color.ToString();
            if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

            SKBitmap skBitmap = getSkBitmapFromSvg(SvgPath, 1f, 1f, color);
            Bitmap bmp = getBitmapFromSKBitmap(skBitmap);

            lock (iconImages)
            {
                iconImages.Add(iconName, bmp);
            }

            return iconImages[iconName];
        }

        //public static Bitmap GetSvgBitmap(
        //    string SvgPath1, Avalonia.Media.Color color1,
        //    string SvgPath2, Avalonia.Media.Color color2
        //    )
        //{
        //    string iconName1 = SvgPath1.Substring(SvgPath1.LastIndexOf('/') + 1);
        //    if (!SvgPath1.ToLower().EndsWith(".svg")) throw new Exception();
        //    iconName1 = iconName1.Substring(0, iconName1.Length - 4);
        //    iconName1 += color1.ToString();

        //    string iconName2 = SvgPath2.Substring(SvgPath2.LastIndexOf('/') + 1);
        //    if (!SvgPath2.ToLower().EndsWith(".svg")) throw new Exception();
        //    iconName2 = iconName2.Substring(0, iconName2.Length - 4);
        //    iconName2 += color2.ToString();

        //    string iconName = iconName1 + iconName2;
        //    if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

        //    SKBitmap skBitmap1 = getSkBitmapFromSvg(SvgPath1, 1f,1f, color1);
        //    SKBitmap skBitmap2 = getSkBitmapFromSvg(SvgPath2, 0.5f,0.5f,color2);

        //    using SKCanvas canvas = new SKCanvas(skBitmap1);
        //    var pixelSize = new PixelSize((int)skBitmap1.Width, (int)skBitmap1.Height);

        //    canvas?.DrawBitmap(skBitmap1, 0, 0);// paint);

        //    using var paint = new SKPaint
        //    {
        //        ColorFilter = SKColorFilter.CreateBlendMode(
        //            color2.ToSKColor(),
        //            SKBlendMode.SrcOver
        //            )
        //    };
        //    canvas?.DrawBitmap(skBitmap2, skBitmap1.Width / 2, 0);//, paint);

        //    Bitmap bmp = getBitmapFromSKBitmap(skBitmap1);

        //    lock (iconImages)
        //    {
        //        iconImages.Add(iconName, bmp);
        //    }

        //    return iconImages[iconName];
        //}

        public enum OverridePosition
        {
            Fill,
            Center,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        }

        public class OverrideIcon
        {
            [SetsRequiredMembers]
            public OverrideIcon() { }
            public required string SvgPath { get; init; }
            public required Avalonia.Media.Color Color { get; init; }
            public required OverridePosition OverridePosition { get; init; }

            public string ID
            {
                get
                {
                    return SvgPath + Color.ToString() + OverridePosition.ToString();
                }
            }
        }

        public static Bitmap GetSvgBitmap(
            string SvgPath1, Avalonia.Media.Color color1,
            string SvgPath2, Avalonia.Media.Color color2
            )
        {
            return GetSvgBitmap(SvgPath1, color1,
                new List<OverrideIcon> { new OverrideIcon() { SvgPath = SvgPath2, Color = color2, OverridePosition = OverridePosition.UpRight } }
                );
        }
        public static Bitmap GetSvgBitmap(
        string SvgPath1, Avalonia.Media.Color color1,
        List<OverrideIcon> overrideIcons
        )
        {
            string iconName = SvgPath1.Substring(SvgPath1.LastIndexOf('/') + 1);
            if (!SvgPath1.ToLower().EndsWith(".svg")) throw new Exception();

            // create cashe key
            iconName = iconName.Substring(0, iconName.Length - 4);
            iconName += color1.ToString();
            foreach (OverrideIcon icon in overrideIcons)
            {
                iconName += icon.ID;
            }
            // return cashed icon if it cashed
            if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

            // creare bitmap
            SKBitmap skBitmap1 = getSkBitmapFromSvg(SvgPath1, 1f, 1f, color1);

            using SKCanvas canvas = new SKCanvas(skBitmap1);
            var pixelSize = new PixelSize((int)skBitmap1.Width, (int)skBitmap1.Height);
            canvas?.DrawBitmap(skBitmap1, 0, 0);

            foreach (OverrideIcon icon in overrideIcons)
            {
                string iconName2 = icon.SvgPath.Substring(icon.SvgPath.LastIndexOf('/') + 1);
                if (!icon.SvgPath.ToLower().EndsWith(".svg")) throw new Exception();
                iconName2 = iconName2.Substring(0, iconName2.Length - 4);
                iconName2 += icon.Color.ToString();
                float size = 1;
                //switch (icon.OverridePosition)
                //{
                //    case OverridePosition.Fill:
                //        size = 1;
                //        break;
                //    default:
                //        size = 0.5f;
                //        break;
                //}
                SKBitmap skBitmap2 = getSkBitmapFromSvg(icon.SvgPath, size, size, icon.Color);
                using var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateBlendMode(
                        icon.Color.ToSKColor(),
                        SKBlendMode.SrcOver
                        )
                };
                switch (icon.OverridePosition)
                {
                    case OverridePosition.Fill:
                    case OverridePosition.UpLeft:
                        canvas?.DrawBitmap(skBitmap2, 0, 0);
                        break;
                    case OverridePosition.UpRight:
                        canvas?.DrawBitmap(skBitmap2, skBitmap1.Width / 2, 0);
                        break;
                    case OverridePosition.Center:
                        canvas?.DrawBitmap(skBitmap2, skBitmap1.Width / 4, skBitmap1.Height / 4);
                        break;
                    case OverridePosition.DownLeft:
                        canvas?.DrawBitmap(skBitmap2, 0, skBitmap1.Height / 2);
                        break;
                    case OverridePosition.DownRight:
                        canvas?.DrawBitmap(skBitmap2, skBitmap1.Width / 2, skBitmap1.Height / 2);
                        break;
                }

            }

            Bitmap bmp = getBitmapFromSKBitmap(skBitmap1);

            lock (iconImages)
            {
                iconImages.Add(iconName, bmp);
            }
            return iconImages[iconName];
        }
        private static Bitmap getBitmapFromSKBitmap(SKBitmap skBitmap)
        {
            Bitmap bmp;
            using (var enc = skBitmap.Encode(SKEncodedImageFormat.Png, 80))
            {
                using (var ms = new MemoryStream())
                {
                    enc.SaveTo(ms);
                    ms.Position = 0;
                    bmp = new Bitmap(ms);
                }
            }
            return bmp;
        }


        private static SKBitmap getSkBitmapFromSvg(string SvgPath, float scaleX, float scaleY)
        {
            // 引数なしコンストラクタを持つ SKSvg を直接利用する
            var svg = new SKSvg();

            using (var stream = AssetLoader.Open(new Uri("avares://" + SvgPath)))
            {
                svg.Load(stream);
            }

            if (svg.Picture == null)
                throw new Exception($"Failed to load SVG from path: {SvgPath}");

            SKBitmap? skBitmap = svg.Picture.ToBitmap(
                SKColors.Transparent,
                scaleX,
                scaleY,
                SKColorType.Bgra8888,
                SKAlphaType.Premul,
                SKColorSpace.CreateSrgb()
            );

            if (skBitmap == null)
                throw new Exception("Failed to convert SVG picture to SKBitmap.");

            return skBitmap;
        }
        private static SKBitmap getSkBitmapFromSvg(string SvgPath, float scaleX, float scaleY, Avalonia.Media.Color color)
        {
            // 1. SVGのロード
            var svg = new SKSvg();
            using (var stream = AssetLoader.Open(new Uri("avares://" + SvgPath)))
            {
                svg.Load(stream);
            }

            if (svg.Picture == null)
                throw new Exception($"Failed to load SVG from path: {SvgPath}");

            // 2. 拡大率を反映した描画サイズを取得
            var width = (int)Math.Ceiling(svg.Picture.CullRect.Width * scaleX);
            var height = (int)Math.Ceiling(svg.Picture.CullRect.Height * scaleY);

            if (width <= 0 || height <= 0)
                throw new Exception("Invalid SVG bounds.");

            // 3. 描画先となるビットマップを生成
            var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using (var canvas = new SKCanvas(skBitmap))
            {
                canvas.Clear(SKColors.Transparent);

                // 4. 指定色に変更するためのカラーフィルタを設定
                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    ColorFilter = SKColorFilter.CreateBlendMode(
                        color.ToSKColor(),
                        SKBlendMode.SrcIn
                    )
                };

                // 5. スケール変換行列を作成して SVG Picture をキャンバスに直接描画
                var matrix = SKMatrix.CreateScale(scaleX, scaleY);
                canvas.DrawPicture(svg.Picture, ref matrix, paint);
            }

            return skBitmap;
        }


    }
}
