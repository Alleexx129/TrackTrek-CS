using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TrackTrek.Miscs;
using YoutubeExplode.Videos;

namespace TrackTrek.Audio
{
    internal class CustomMetaData
    {
       public static async Task Add(string path, dynamic imageUrl, string artist, string title, string? albumName = "")
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
            file.Tag.Performers = new string[] { Filter.FilterArtistName(artist) };
            file.Tag.Title = title;
            file.Tag.Album = albumName;

            file.Save();
        }
    
        public static async Task<byte[]> DownloadThumbnailAsBytes(object url)
            {
                if (url is byte[] byteArray)
                {
                        return byteArray;
                } else if (url is string videoUrl)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        return await client.GetByteArrayAsync(videoUrl);
                    }
                } else
                {
                return new byte[0];
                }
                
            }
        }
    }
