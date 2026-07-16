using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Media.Imaging;
using static NewScarAnime.BangumiSearch;
using static NewScarAnime.HomePage;


namespace MoeSoft.Services;


public class ImageService
{
    private readonly HttpClient client;


    public ImageService()
    {
        var handler = new HttpClientHandler()
        {
            Proxy = new WebProxy(
                "socks5://127.0.0.1:10808"
            ),

            UseProxy = true
        };


        client = new HttpClient(handler);
    }



    public async Task LoadImages(
        List<BangumiSearchResult> results)
    {

        foreach (var item in results)
        {
            try
            {
                byte[] data =
                    await client.GetByteArrayAsync(
                        item.image
                    );


                using MemoryStream ms =
                    new(data);


                BitmapImage bitmap =
                    new();


                bitmap.BeginInit();

                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;

                bitmap.StreamSource =
                    ms;

                bitmap.EndInit();


                bitmap.Freeze();


                item.CoverImage = bitmap;

            }
            catch
            {
                item.CoverImage =
                    new BitmapImage(
                        new Uri(
                        "pack://application:,,,/Icon/DontFindImage.png"
                        )
                    );
            }
        }
    }
}