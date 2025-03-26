using HtmlAgilityPack;
using TrackTrek.Miscs;
using YoutubeExplode.Videos;

namespace TrackTrek.Audio
{
    internal class CustomMetaData
    {
       public static async Task Add(string path, string imageUrl, Video videoInfo)
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
            file.Tag.Performers = new string[] { FilterArtistName.filter(videoInfo.Author) };
            file.Tag.Title = videoInfo.Title;

            file.Save();
        }
    
        public static async Task<byte[]> DownloadThumbnailAsBytes(string url)
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetByteArrayAsync(url);
                }
            }
        }
    }
