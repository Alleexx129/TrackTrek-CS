using System;
using System.Drawing;
using System.IO;

namespace TrackTrek.Miscs
{
    internal class ImageUtils
    {
        public static byte[] ResizeImage(byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            using (var img = Image.FromStream(ms))
            {
                int width = img.Width;
                int height = img.Height;

                int newSize = Math.Min(width, height);
                int left = (width - newSize) / 2;
                int top = 0;
                int right = (width + newSize) / 2;
                int bottom = height;

                using (var croppedImage = new Bitmap(newSize, newSize))
                using (var g = Graphics.FromImage(croppedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    g.DrawImage(img, new Rectangle(0, 0, newSize, newSize),
                        new Rectangle(left, top, newSize, newSize),
                        GraphicsUnit.Pixel);

                    using (var outputMs = new MemoryStream())
                    {
                        croppedImage.Save(outputMs, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return outputMs.ToArray();
                    }
                }
            }
        }
    }
}
