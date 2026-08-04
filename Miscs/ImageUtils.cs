using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SkiaSharp;

namespace TrackTrek.Miscs
{
    internal class ImageUtils
    {
        public static byte[] ResizeImage(byte[] imageBytes)
        {
            using (SKBitmap img = SKBitmap.Decode(imageBytes))
            {
                int width = img.Width;
                int height = img.Height;

                int newSize = Math.Min(width, height);
                int left = (width - newSize) / 2;
                int top = (height - newSize) / 2;

                var srcRect = SKRectI.Create(left, top, newSize, newSize);
                var destRect = SKRect.Create(0, 0, newSize, newSize);

                using (var croppedImage = new SKBitmap(newSize, newSize))
                {
                    using (var canvas = new SKCanvas(croppedImage))
                    {
                        canvas.DrawBitmap(img, srcRect, destRect);
                    }

                    using (var image = SKImage.FromBitmap(croppedImage))
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                    {
                        return data.ToArray();
                    }
                }
            }
        }


        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetAlbumImageFromSongPage(string songAlbumUrl)
        {
            int attempt = 0;

            while (attempt < 5)
            {
                attempt++;
                try
                {
                    var html = await client.GetStringAsync(songAlbumUrl);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                    return imageNode?.GetAttributeValue("content", "") ?? "";
                }
                catch (Exception ex)
                {
                    Sys.debug(ex.Message.ToString());
                }
                Task.Delay(attempt * 1000);
            }
            return "";
        }

        public static async Task<string> GetAlbumImageUrl(string albumName, string artistName)
        {
            string albumUrl = $"https://www.last.fm/music/{artistName.Replace(" ", "+")}/{albumName.Replace(" ", "+")}/+images";
            return albumUrl;
        }

        public static async Task<string> GetSoundCloudUrl(string title, string artist)
        {
            string albumUrl = $"https://soundcloud.com/{artist.Replace(" ", "-").ToLower()}/{title.Replace(" ", "-").ToLower()}";
            return albumUrl;
        }
    }
}