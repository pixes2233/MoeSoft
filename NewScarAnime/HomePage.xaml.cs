using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewScarAnime
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        private static Visibility _progressBarState = Visibility.Hidden;
        public ObservableCollection<AnimeItem> Anime { get; set; }
        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
            Anime = new ObservableCollection<AnimeItem>();
            LoadItems();
            ProgressBar.Visibility = _progressBarState;
        }

        public class AnimeInfo
        {
            /// <summary>
            /// アニメjson中的数据类型
            /// </summary>
            public string bangumi_url { get; set; } // Bangumi 的 URL
            public string name { get; set; } // 动漫标题
            public string name_chinese { get; set; } // 中文标题
            public string image_url { get; set; } // 封面图片的 URL
            public string summary { get; set; } // 动漫简介
            public int total_episodes { get; set; } // 总集数
            public string start_date { get; set; } // 首播日期
            public string air_weekday { get; set; } // 首播星期几
            public string director { get; set; } // 导演
            public string writer { get; set; } // 编剧
            public string studio { get; set; } // 制作公司
            public int current_episode { get; set; } // 当前集数
        }

        public class AnimeItem
        {
            public string AnimeTitleCN { get; set; } // 动漫中文标题
            public DateOnly StartDate { get; set; } // 动漫首播日期
            public ImageSource AnimeCover { get; set; } // 动漫封面图片的 URL
            public string BangumiID { get; set; } // Bangumi ID
            public bool IsCurrentSeason { get; set; }
        }

        private static string GetLocalAddress()
        {
            ///<summary>
            ///返回用户本地应用程序数据文件夹路径
            /// </summary>

            // 获取本地应用程序数据文件夹路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 构建应用程序专用子文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");

            return appSpecificFolder;
        }

        private void LoadItems()
        {
            ///<summary>
            ///载入所有AnimeInfo数据并显示在首页
            /// </summary>

            Anime.Clear();
            foreach (var info in LoadAllAnimeInfo())
            {
                string title = info.name_chinese;

                DateOnly startDate = new DateOnly(0001, 1, 1);

                if (info.start_date != "*")
                {
                    startDate = DateOnly.ParseExact(info.start_date, "yyyy年M月d日", null);
                }

                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeCover");

                string bangumiId = ExtractSubjectId(info.bangumi_url);

                string fileName = $"{bangumiId}.jpg"; // 使用提取的ID作为文件名

                string coverPath = System.IO.Path.Combine(appSpecificFolder, fileName);

                // 创建一个 BitmapImage 作为图像源
                BitmapImage bitmapImage = new BitmapImage();

                if (System.IO.File.Exists(coverPath))
                {
                    try
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(coverPath, UriKind.Absolute);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 这确保文件不会被锁定
                        bitmapImage.EndInit();
                    }
                    catch (Exception ex)
                    {
                        // 处理图像文件可能损坏或不可读的情况
                        new Wpf.Ui.Controls.MessageBox { Title = "System", Content = $"加载图片失败: {coverPath}\n错误: {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
                        // （可选）设置一个占位符图像或 null
                        bitmapImage = null;
                    }
                }
                else
                {
                    new Wpf.Ui.Controls.MessageBox { Title = "查找封面异常", Content = $"封面图片未找到: {coverPath}", CloseButtonText = "确定" }.ShowDialogAsync();
                    // （可选）设置一个占位符图像或 null
                    bitmapImage = null;
                }

                Anime.Add(new AnimeItem
                {
                    AnimeTitleCN = title,
                    StartDate = startDate,
                    AnimeCover = bitmapImage,
                    BangumiID = bangumiId,
                    IsCurrentSeason = false
                });

                // 本季度番剧显示逻辑
                for (int i = 0; i < Anime.Count; i++)
                {
                    DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                    var animeItem = Anime[i];
                    var season = GetAnimeSeason(animeItem.StartDate);

                    if (IsCurrentSeason(animeItem.StartDate))
                    {
                        Anime[i].IsCurrentSeason = true;
                        Anime.Move(i, 0);
                    }
                }
            }
        }

        public static bool IsCurrentSeason(DateOnly animeDate, int bufferDays = 7)
        {
            ///<summary>
            ///判断是否是本季度番剧
            ///</summary>
            var today = DateOnly.FromDateTime(DateTime.Today);

            var animeSeason = GetAnimeSeason(animeDate, bufferDays);
            var currentSeason = GetAnimeSeason(today, bufferDays);

            return animeSeason.Year == currentSeason.Year && animeSeason.Quarter == currentSeason.Quarter;
        }

        public static (int Year, int Quarter) GetAnimeSeason(DateOnly date, int bufferDays = 7)
        {
            ///<summary>
            ///判断是否是本季度番剧の主逻辑
            ///</summary>

            // 季度起始月
            int[] quarterStartMonths = { 1, 4, 7, 10 };

            for (int i = 0; i < quarterStartMonths.Length; i++)
            {
                int startMonth = quarterStartMonths[i];

                // 当前季度开始时间
                var seasonStart = new DateOnly(date.Year, startMonth, 1);

                // 如果是第一季度，需要看上一年的10月
                if (i == 0)
                {
                    seasonStart = new DateOnly(date.Year, 1, 1);
                }

                // 下一个季度开始时间
                DateOnly nextSeasonStart;
                if (i == 3)
                {
                    nextSeasonStart = new DateOnly(date.Year + 1, 1, 1);
                }
                else
                {
                    nextSeasonStart = new DateOnly(date.Year, quarterStartMonths[i + 1], 1);
                }

                // 👇 关键：给“下个季度”留提前空间
                var adjustedNextSeasonStart = nextSeasonStart.AddDays(-bufferDays);

                if (date >= seasonStart && date < adjustedNextSeasonStart)
                {
                    return (date.Year, i + 1);
                }

                // 👇 提前进入下个季度
                if (date >= adjustedNextSeasonStart && date < nextSeasonStart)
                {
                    int nextQuarter = (i + 1) % 4 + 1;
                    int nextYear = (i == 3) ? date.Year + 1 : date.Year;
                    return (nextYear, nextQuarter);
                }
            }

            // 理论不会走到这里
            return (date.Year, (date.Month - 1) / 3 + 1);
        }

        public static List<AnimeInfo> LoadAllAnimeInfo()
        {
            /// <summary>
            /// 一次性返回所有的AnimeInfo数据
            /// </summary>
            
            var animeList = new List<AnimeInfo>();
            string folderPath = System.IO.Path.Combine(GetLocalAddress(), "AnimeInfo");

            if (!Directory.Exists(folderPath))
                return animeList;

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(file);
                    AnimeInfo info = JsonConvert.DeserializeObject<AnimeInfo>(json);
                    if (info != null)
                    {
                        animeList.Add(info);
                    }
                    else
                    {
                        new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"文件内容为空或格式不正确: {file}", CloseButtonText = "确定" }.ShowDialogAsync();
                    }
                }
                catch
                {
                    new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"无法解析文件: {file}", CloseButtonText = "确定" }.ShowDialogAsync();
                }
            }
            return animeList;
        }

        public static async Task DownloadImageSync(string imageUrl, string fileName)
        {
            /// <summary>
            /// 下载bangumi提供的动漫封面图片
            /// </summary>
            string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeCover");
            string filePath = System.IO.Path.Combine(appSpecificFolder, fileName);

            // 检查该子文件夹是否存在，如果不存在则创建它。
            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }

            // 创建 HttpClient 实例
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 1. 下载图片内容作为字节数组（同步版本）
                    // .Result 会阻塞当前线程，直到 GetByteArrayAsync 完成。
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // 2. 将字节数组写入本地文件（同步版本）
                    await Task.Run(() => System.IO.File.WriteAllBytes(filePath, imageBytes));
                }
                catch (HttpRequestException httpEx)
                {
                    new Wpf.Ui.Controls.MessageBox { Title = "网络异常", Content = $"下载失败 (网络/HTTP 错误): {httpEx.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
                }
                catch (Exception ex)
                {
                    new Wpf.Ui.Controls.MessageBox { Title = "系统异常", Content = $"保存失败 (未知错误): {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
                }
            }
        }

        public static string ExtractSubjectId(string url)
        {
            /// <summary>
            /// 获取Bangumi URL 中的 Subject ID
            /// </summary>
            // 1. 使用 Uri 类解析 URL
            Uri uri = new Uri(url);

            // 2. 获取 URL 的路径部分
            // 对于 "https://bangumi.tv/subject/390555"，PathAndQuery 会返回 "/subject/390555"
            string path = uri.AbsolutePath;

            // 3. 使用正则表达式提取数字
            Match match = Regex.Match(path, @"/subject/(\d+)$");

            // 4. 检查是否匹配成功
            if (match.Success && match.Groups.Count > 1)
            {
                // match.Groups[1] 包含第一个捕获组的内容，也就是括号()里的数字
                return match.Groups[1].Value;
            }
            else
            {
                // 如果没有匹配到，返回空字符串或抛出异常，根据具体需求而定
                return string.Empty;
            }
        }

        // 解析 JSON 并获取 AnimeInfo 中的 Cover 字段
        public static string ParseAnimeImageUrl(string jsonData)
        {
            try
            {
                // JSON 字符串反序列化为 User 对象
                AnimeInfo deserializedUser = JsonConvert.DeserializeObject<AnimeInfo>(jsonData);
                return deserializedUser?.image_url ?? string.Empty; // 如果 deserializedUser 为 null，则返回空字符串
            }
            catch (Exception ex)
            {
                new Wpf.Ui.Controls.MessageBox { Title = "解析异常", Content = $"解析 JSON 时出错: {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
                return string.Empty;
            }
        }

        public static async Task RunBangumiScraper(string url)
        {
            ///<summary>
            ///启动爬虫程序
            /// </summary>

            // 指定要启动的exe文件路径
            var exePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Scrap", "BangumiScraper.exe");

            // 要传递给exe的参数
            string argument = url; // 这是你要作为命令行参数传递的字符串

            // 创建进程启动信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = argument, // *** 新增：设置命令行参数 ***
                RedirectStandardInput = false, // *** 修改：不再需要重定向标准输入 ***
                RedirectStandardError = true, // *** 新增：重定向标准错误输出 ***
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8 // *** 新增：设置标准输出编码为UTF8 ***
            };

            try
            {
                // 启动进程
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // 读取exe文件的输出和错误
                    using (StreamReader outputReader = process.StandardOutput)
                    {
                        // 异步读取输出和错误流，不阻塞当前线程
                        Task<string> outputReadTask = process.StandardOutput.ReadToEndAsync();
                        Task<string> errorReadTask = process.StandardError.ReadToEndAsync();

                        // 异步等待进程退出
                        await Task.Run(() => process.WaitForExit()); // 将 WaitForExit 放到后台线程

                        string output = await outputReadTask; // 等待输出读取完成
                        string error = (await errorReadTask).Trim(); // 等待错误读取完成并 Trim

                        if (error == "Done")
                        {
                            string fileName = ExtractSubjectId(argument); // 使用提取的ID作为文件名
                            SaveJsonOutput(output, fileName + ".json"); // 保存输出到JSON文件
                            await DownloadImageSync(ParseAnimeImageUrl(output), fileName + ".jpg"); // 下载图片并保存
                        }
                        else
                        {
                            new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"Exe Error: {error}", CloseButtonText = "确定" }.ShowDialogAsync();
                        }
                    }

                    process.WaitForExit(); // 等待进程退出
                }
            }
            catch (Exception ex)
            {
                new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"Error: {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
            }
        }

        public static void SaveJsonOutput(string jsonData, string fileName)
        {
            try
            {
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeInfo");

                // 检查该子文件夹是否存在，如果不存在则创建它。
                if (!Directory.Exists(appSpecificFolder))
                {
                    Directory.CreateDirectory(appSpecificFolder);
                }

                // 3. 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, fileName);

                // 4. 将 JSON 数据写入文件
                // 如果文件不存在，它会创建文件；如果文件已存在，它会覆盖现有内容。
                System.IO.File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                // 6. 显示错误消息
                new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"保存 JSON 数据时出错: {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
            }
        }

        public static string LoadJsonOutput(string fileName = "user_data.json")
        {
            string jsonData = string.Empty; // 初始化一个空字符串，用于存储加载的数据
            try
            {
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeInfo");
                string filePath = System.IO.Path.Combine(appSpecificFolder, fileName);

                // 检查文件是否存在
                if (System.IO.File.Exists(filePath))
                {
                    // 从文件读取 JSON 数据
                    // File.ReadAllText 读取指定文件的所有文本内容，并将其作为单个字符串返回。
                    jsonData = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = "未找到保存的 JSON 数据。", CloseButtonText = "确定" }.ShowDialogAsync();
                }
            }
            catch (Exception ex)
            { 
                new Wpf.Ui.Controls.MessageBox { Title = "文件异常", Content = $"加载 JSON 数据时出错: {ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
            }
            return jsonData; // 返回加载到的 JSON 字符串（如果未找到或出错，则返回空字符串）
        }

        private void FileLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private async void UpdateAnimeInfo(object sender, RoutedEventArgs e)
        {
            ///<summary>
            ///更新所有AnimeInfo数据
            /// </summary>
            _progressBarState = Visibility.Visible;
            ProgressBar.Visibility = _progressBarState;
            var infos = LoadAllAnimeInfo();
            var tasks = new List<Task>();
            using var sem = new SemaphoreSlim(4); // 最多并发4个
            foreach (var info in infos)
            {
                await sem.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await RunBangumiScraper(info.bangumi_url);
                    }
                    finally
                    {
                        sem.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);

            _progressBarState = Visibility.Hidden;
            ProgressBar.Visibility = _progressBarState;
        }

        private void OpenAnimeInfo(object sender, RoutedEventArgs e)
        {
            // 获取触发事件的 CardAction 控件
            Wpf.Ui.Controls.CardAction clickedCardAction = sender as Wpf.Ui.Controls.CardAction;

            // 确保点击的是 CardAction 控件并且 DataContext 非空
            if (clickedCardAction != null && clickedCardAction.DataContext is AnimeItem clickedAnime)
            {
                string bangumiId = clickedAnime.BangumiID;

                // 使用 bangumiId 导航到 AnimeInfoPage
                this.NavigationService.Content = new AnimeInfoPage(bangumiId);
            }

            e.Handled = true;
        }
    }
}
