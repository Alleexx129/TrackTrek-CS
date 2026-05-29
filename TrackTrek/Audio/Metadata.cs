using System.Net;
using TrackTrek.Miscs;

namespace TrackTrek.Audio
{
    internal class CustomMetaData
    {
        public static async Task Add(Searching.VideoInfo videoInfo)
        {
            TagLib.File file = TagLib.File.Create(videoInfo.Path);
            byte[] imageBytes = await DownloadThumbnailAsBytes(videoInfo.AlbumImageUrl);

            if (imageBytes is [4,0,4])
            {
                imageBytes = await DownloadThumbnailAsBytes(videoInfo.YoutubeImageUrl);
            }


            var picture = new TagLib.Picture
            {
                Type = TagLib.PictureType.FrontCover,
                MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                Data = new TagLib.ByteVector(ImageUtils.ResizeImage(imageBytes))
            };

            file.Tag.Pictures = new TagLib.IPicture[] { picture };
            file.Tag.Performers = new string[] {Filter.FilterArtist(videoInfo.GetArtist())};
            file.Tag.Title = videoInfo.Title;
            file.Tag.Album = videoInfo.Album;
            file.Tag.Lyrics = videoInfo.Lyrics;

            file.Save(); // used by another process error
        }

        public static async Task<byte[]> DownloadThumbnailAsBytes(object url)
        {
            
            if (url is byte[] byteArray)
            {
                Sys.debug("Image is byte array of " + byteArray.Length.ToString() + "Characters");
                return byteArray;
            }
            else if (url is string imageUrl)
            {
                Sys.debug(imageUrl);
                if (imageUrl.Trim().EndsWith(".png", StringComparison.OrdinalIgnoreCase) || imageUrl.Trim().EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || imageUrl.Trim().EndsWith(".webp", StringComparison.OrdinalIgnoreCase))

                {
                    using (HttpClient client = new HttpClient())
                    {
                        return await client.GetByteArrayAsync(imageUrl);
                    }
                } else
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string html = "";
                        bool error = false;

                        for (int i = 0;i<=10;i++)
                        {
                            try
                            {
                                html = await client.GetStringAsync(imageUrl);
                                error = false;
                                break;
                            }
                            catch (HttpRequestException e)
                            {
                                Sys.debug($"Unexpected error occured: {e}");

                                if (e.StatusCode == HttpStatusCode.NotFound) // artists not being able to name their album properly...
                                {
                                    Sys.debug("Not retrying since its 404");

                                    return (byte[])([4,0,4]); // return "1" if error instead in the future
                                }
                                Sys.debug($"Retrying... {i}");
                                error = true;
                                await Task.Delay(2000);
                            }
                        }
                        
                        if (error == true)
                        {
                            return await CustomMetaData.DownloadThumbnailAsBytes("https://r2.image-upload.app/uploads/permanent/image/1771522735192-i59pdc6gfmd.png");
                        }
                        
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        
                        doc.LoadHtml(html);

                        var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                        Sys.debug($"Image url: {imageUrl}");
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
