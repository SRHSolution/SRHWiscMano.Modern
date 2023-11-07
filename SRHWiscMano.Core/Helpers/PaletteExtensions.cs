using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SRHWiscMano.Core.Helpers
{
    public static class PaletteExtensions
    {
        /// <summary>
        /// Palette의 색정보를 바탕으로 oxyimage 데이터를 생성하여 전달한다.
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="isHorizontal"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static OxyImage GenerateColorAxisImage(this OxyPalette palette, bool isHorizontal,  bool reverse)
        {
            int n = palette.Colors.Count;
            var buffer = isHorizontal ? new OxyColor[n, 1] : new OxyColor[1, n];
            for (var i = 0; i < n; i++)
            {
                var color = palette.Colors[i];
                var i2 = reverse ? n - 1 - i : i;
                if (isHorizontal)
                {
                    buffer[i2, 0] = color;
                }
                else
                {
                    buffer[0, i2] = color;
                }
            }

            return OxyImage.Create(buffer, ImageFormat.Png);
        }

        /// <summary>
        /// OxyImage 를 입력된 width, height 크기의 bitmap 이미지로 변환하여 전달한다.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static BitmapSource GetResizedImage(this OxyImage image, int width, int height)
        {
            using var ms = new MemoryStream(image.GetData());
            var btm = new BitmapImage();
            btm.BeginInit();
            btm.StreamSource = ms;
            btm.CacheOption = BitmapCacheOption.OnLoad;
            btm.EndInit();
            btm.Freeze();

            return ResizeImage(btm, width, height);
        }

        private static BitmapSource ResizeImage(BitmapSource source, int width, int height)
        {
            // Calculate the new height and width
            double scaleX = width / source.Width;
            double scaleY = height / source.Height;
            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);

            // Create a TransformedBitmap to apply the scale transform
            // Note that this approach may result in lower quality
            TransformedBitmap transformedBitmap = new TransformedBitmap(source, scaleTransform);
            transformedBitmap.Freeze(); // Freeze the bitmap for performance benefits

            return transformedBitmap;
        }
    }
}
