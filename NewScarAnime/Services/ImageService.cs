using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using static MoeSoft.BangumiSearch;
using static MoeSoft.HomePage;


namespace MoeSoft.Services;


public class ImageService
{
    public async Task LoadImages(
        List<BangumiSearchResult> results)
    {
        using var client = new HttpClient(
            ProxyHandlerFactory.CreateHandler(
                GlobalConfig.IsProxyEnabled,
                GlobalConfig.ProxyAddress));

        var fallbackImage = new BitmapImage(
            new Uri("pack://application:,,,/Icon/DontFindImage.png"));
        fallbackImage.Freeze();

        // 限制并发数，避免搜索结果较多时同时建立过多连接。
        await Parallel.ForEachAsync(
            results,
            new ParallelOptions { MaxDegreeOfParallelism = 4 },
            async (item, cancellationToken) =>
            {
                try
                {
                    byte[] data = await client.GetByteArrayAsync(item.image, cancellationToken);
                    using MemoryStream ms = new(data);
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    item.CoverImage = bitmap;
                }
                catch
                {
                    item.CoverImage = fallbackImage;
                }
            });
    }
}