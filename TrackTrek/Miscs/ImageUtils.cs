using HtmlAgilityPack;


namespace TrackTrek.Miscs
{
    internal class ImageUtils
    {
        public static byte[] ResizeImage(byte[] imageBytes)
        {
            MemoryStream ms = new MemoryStream(imageBytes);
            using (Image img = Image.FromStream(ms))
            {
                int width = img.Width;
                int height = img.Height;

                int newSize = Math.Min(width, height);
                int left = (width - newSize) / 2;
                int top = (height - newSize) / 2;

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
        private static readonly HttpClient client = new HttpClient();

        private protected static async Task<string> GetAlbumImageFromSongPage(string songAlbumUrl)
        {
            int attempt = 0;

            while (attempt < 5)
            {
                try
                {
                    attempt++;
                    var html = await client.GetStringAsync(songAlbumUrl);
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                    return imageNode?.GetAttributeValue("content", "") ?? "";
                } catch
                {
                }
            }
            return "";
        }

        public static async Task<string> GetAlbumImageUrl(string albumName, string artistName)
        {
            string albumUrl = $"https://www.last.fm/music/{artistName.Replace(" ", "+")}/{albumName.Replace(" ", "+")}/+images";
            return await GetAlbumImageFromSongPage(albumUrl);
        }
    }

    }
