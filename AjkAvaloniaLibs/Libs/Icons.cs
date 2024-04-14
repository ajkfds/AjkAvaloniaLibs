using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Platform;
using System.IO;

// SKPictureに対する拡張メソッドとしてSKPicture.ToImageが定義されている。これを使うのに必要。
using SkiaSharp;
using Svg.Skia;
using Avalonia.Skia;
using Avalonia;

namespace AjkAvaloniaLibs.Libs
{
    public static class Icons
    {
        // 一度使ったimageは保持しておく
        public static Dictionary<string, IImage> iconImages = new Dictionary<string, IImage>();

        public enum ColorStyle : int
        {
            Original = 0,
            Gray = 1,
            Red = 2,
            Blue = 3,
            Green = 4,
            White = 5,
            Orange = 6,
        }

        /// <summary>
        /// Assets/Icons/以下の(iconName).svgをIImage形式にして取り出す。
        /// 一度使ったIImageはキャッシュする。
        /// </summary>
        public static IImage? GetSvgBitmap(string SvgPath)
        {
            string iconName = SvgPath.Substring(SvgPath.LastIndexOf('/') + 1);
            if (!SvgPath.ToLower().EndsWith(".svg")) throw new Exception();
            iconName = iconName.Substring(0, iconName.Length - 4);

            if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

                // Asset/Icons化のiconName".svg"をstringとして取得する
                string svgString;
                using (var stream = AssetLoader.Open(new Uri("avares://" + SvgPath)))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    var encoding = Encoding.GetEncoding("UTF-8");
                    svgString = encoding.GetString(buffer);
                }

                // SvgStringをiconName.pngにいったん書き出す

                Avalonia.Svg.Skia.Svg svg = new Avalonia.Svg.Skia.Svg(new Uri("avares://" + SvgPath));
                //　ここのbaseUriは意味がよくわからない。わからないけどとりあえずsvgファイルのパスを食わせている。
                //　でもこれでsvgが読み込まれるわけではないよう
                svg.Source = svgString;
                //　SourceにsvgStringを入れるとsvg.Pictureが使えるようになる。
                if (svg.Picture == null) return null;

                //// svg.Picture.ToBitmap(SKColors.Transparent, 1f, 1f, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
                //// でSKBitmapを取得できるのだが、これをAvalonia.Media.Imaging.Bitmapに変換する方法がわからない。
                //// しょうがないのでいったんpngファイルに書き出す。
                //using (var stream = File.OpenWrite(iconName + ".png"))
                //{
                //    svg.Picture.ToImage(stream, SKColors.Transparent, SKEncodedImageFormat.Png, 100, 1f, 1f, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
                //}


                //// 書いたpngファイルをAvalonia.Media.Imaging.Bitmapとして読み込む。
                //using (var stream = File.OpenRead(iconName + ".png"))
                //{
                //    Bitmap bmp = new Bitmap(stream);
                //    iconImages.Add(iconName, bmp);
                //}

                // 下記に記載のあった方法でファイルを経由せずにSKBitmapをBitmapに変換できる
                // https://github.com/AvaloniaUI/Avalonia/discussions/13610
                SKBitmap? skBitmap = svg.Picture.ToBitmap(SKColors.Transparent, 1f, 1f, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
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

            lock (iconImages)
            {
                iconImages.Add(iconName, bmp);
            }

            return iconImages[iconName];
        }

        public static IImage? GetSvgBitmap(string SvgPath, Avalonia.Media.Color color)
        {
            string iconName = SvgPath.Substring(SvgPath.LastIndexOf('/') + 1);
            if (!SvgPath.ToLower().EndsWith(".svg")) throw new Exception();
            iconName = iconName.Substring(0, iconName.Length - 4);
            iconName += color.ToString();
            if (iconImages.ContainsKey(iconName)) return iconImages[iconName];

            // Asset/Icons化のiconName".svg"をstringとして取得する
            string svgString;
            using (var stream = AssetLoader.Open(new Uri("avares://" + SvgPath)))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                var encoding = Encoding.GetEncoding("UTF-8");
                svgString = encoding.GetString(buffer);
            }

            // SvgStringをiconName.pngにいったん書き出す

            Avalonia.Svg.Skia.Svg svg = new Avalonia.Svg.Skia.Svg(new Uri("avares://" + SvgPath));
            //　ここのbaseUriは意味がよくわからない。わからないけどとりあえずsvgファイルのパスを食わせている。
            //　でもこれでsvgが読み込まれるわけではないよう
            svg.Source = svgString;
            //　SourceにsvgStringを入れるとsvg.Pictureが使えるようになる。
            if (svg.Picture == null) return null;

            // svg.Picture.ToBitmap(SKColors.Transparent, 1f, 1f, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            // でSKBitmapを取得できるのだが、これをAvalonia.Media.Imaging.Bitmapに変換する方法がわからない。
            // しょうがないのでいったんpngファイルに書き出す。
            SKBitmap skBitmap = svg.Picture.ToBitmap(SKColors.Transparent, 1f, 1f, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            //var interpolationMode = RenderOptions.GetBitmapInterpolationMode(this);
            var pixelSize = new PixelSize((int)skBitmap.Width, (int)skBitmap.Height);
            var dpi = new Vector(96, 96);

            using SKCanvas canvas = new SKCanvas(skBitmap);

            using var colorizedRenderTarget =
                new RenderTargetBitmap(pixelSize, dpi);
            using var colorizedContextImpl = colorizedRenderTarget.CreateDrawingContext();
            using var colorizedSkiaContext = colorizedContextImpl;

            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateBlendMode(
                    color.ToSKColor(),
                    SKBlendMode.SrcIn)
            };

            canvas?.DrawBitmap(skBitmap, 0, 0, paint);

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

            lock (iconImages)
            {
                iconImages.Add(iconName, bmp);
            }

            return iconImages[iconName];
        }
    }
}