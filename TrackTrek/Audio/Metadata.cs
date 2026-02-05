using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TrackTrek.Miscs;
using YoutubeExplode.Videos;

namespace TrackTrek.Audio
{
    internal class CustomMetaData
    {
        public static async Task Add(string path, dynamic imageUrl, string artist, string title, string lyrics, string? albumName = "")
        {
            //string lyricsUrl;
            TagLib.File file = TagLib.File.Create(path);
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml(lyricsUrl);
            byte[] imageBytes = await DownloadThumbnailAsBytes(imageUrl);


            var picture = new TagLib.Picture
            {
                Type = TagLib.PictureType.FrontCover,
                MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                Data = new TagLib.ByteVector(ImageUtils.ResizeImage(imageBytes))
            };

            file.Tag.Pictures = new TagLib.IPicture[] { picture };
            file.Tag.Performers = new string[] { Filter.FilterArtistName(artist).toCapitalFirst() };
            file.Tag.Title = title;
            file.Tag.Album = albumName;
            file.Tag.Lyrics = lyrics;

            file.Save();
        }

        public static async Task<byte[]> DownloadThumbnailAsBytes(object url)
        {
            Sys.debug(url);
            if (url is byte[] byteArray)
            {
                return byteArray;
            }
            else if (url is string videoUrl)
            {
                if (videoUrl.EndsWith(".png") || videoUrl.EndsWith(".jpg") || videoUrl.EndsWith(".webp"))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        return await client.GetByteArrayAsync(videoUrl);
                    }
                } else
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var html = await client.GetStringAsync(videoUrl);
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);

                        var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                        // IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("image-list-item"));
                        // var imgNode = nodes.First();
                        // var newhtml = await client.GetStringAsync("https://last.fm" + imgNode.GetAttributeValue("href", null));
                        // doc.LoadHtml(newhtml);
                        // var link = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("js-gallery-image")).First().GetAttributeValue("src", null);

                        return await client.GetByteArrayAsync(imageNode?.GetAttributeValue("content", "") ?? "");
                    }
                }

            }
            else
            {
                MessageBox.Show("FATAL ERROR");
                return new byte[0];
            }

        }
    }
}
