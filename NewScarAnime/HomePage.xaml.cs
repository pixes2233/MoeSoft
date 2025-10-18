using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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

namespace NewScarAnime
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public ObservableCollection<AnimeItem> Anime { get; set; }
        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
            Anime = new ObservableCollection<AnimeItem>();
            LoadItems();
        }

        public class AnimeItem
        {
            public string AnimeTitleCN { get; set; } // 动漫中文标题
            public ImageSource AnimeCover { get; set; } // 动漫封面图片的 URL
            public string BangumiID { get; set; } // Bangumi ID
        }

        private static string GetLocalAddress()
        {
            // 获取本地应用程序数据文件夹路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 构建应用程序专用子文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");

            return appSpecificFolder;
        }

        private void LoadItems()
        {
            Anime.Clear();
            foreach (var info in LoadAllAnimeInfo())
            {
                string title = info.name_chinese;

                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeCover");

                string bangumiId = ExtractSubjectId(info.bangumi_url);

                string fileName = $"{bangumiId}.jpg"; // 使用提取的ID作为文件名

                string coverPath = System.IO.Path.Combine(appSpecificFolder, fileName);

                // 创建一个 BitmapImage 作为图像源
                BitmapImage bitmapImage = new BitmapImage();

                if (File.Exists(coverPath))
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
                        MessageBox.Show($"加载图片失败: {coverPath}\n错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        // （可选）设置一个占位符图像或 null
                        bitmapImage = null;
                    }
                }
                else
                {
                    MessageBox.Show($"封面图片未找到: {coverPath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    // （可选）设置一个占位符图像或 null
                    bitmapImage = null;
                }

                Anime.Add(new AnimeItem
                {
                    AnimeTitleCN = title,
                    AnimeCover = bitmapImage,
                    BangumiID = bangumiId
                });
            }
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
                    string json = File.ReadAllText(file);
                    AnimeInfo info = JsonConvert.DeserializeObject<AnimeInfo>(json);
                    if (info != null)
                    {
                        animeList.Add(info);
                    }
                    else
                    {
                        MessageBox.Show($"文件内容为空或格式不正确: {file}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    MessageBox.Show($"无法解析文件: {file}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return animeList;
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
                    await Task.Run(() => File.WriteAllBytes(filePath, imageBytes));
                }
                catch (HttpRequestException httpEx)
                {
                    MessageBox.Show($"下载失败 (网络/HTTP 错误): {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败 (未知错误): {ex.Message}");
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
                MessageBox.Show($"解析 JSON 时出错: {ex.Message}", "解析错误");
                return string.Empty;
            }
        }

        public static async Task RunBangumiScraper(string url)
        {
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
                            MessageBox.Show($"Exe Error: {error}");
                        }
                    }

                    process.WaitForExit(); // 等待进程退出
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                // 6. 显示错误消息
                MessageBox.Show($"保存 JSON 数据时出错: {ex.Message}", "保存错误");
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
                if (File.Exists(filePath))
                {
                    // 从文件读取 JSON 数据
                    // File.ReadAllText 读取指定文件的所有文本内容，并将其作为单个字符串返回。
                    jsonData = File.ReadAllText(filePath);
                }
                else
                {
                    MessageBox.Show("未找到保存的 JSON 数据。", "加载信息");
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show($"加载 JSON 数据时出错: {ex.Message}", "加载错误");
            }
            return jsonData; // 返回加载到的 JSON 字符串（如果未找到或出错，则返回空字符串）
        }

        private void OpenAnimeInfo(object sender, MouseButtonEventArgs e)
        {
            Border clickedBorder = sender as Border;

            // 获取 Border 的 DataContext，它就是与该 Border 绑定的 AnimeItem 对象
            AnimeItem clickedAnime = clickedBorder.DataContext as AnimeItem;

            string bangumiId = clickedAnime.BangumiID;

            this.NavigationService.Content = new AnimeInfoPage(bangumiId);

            e.Handled = true;
        }

        private void FileLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void UpdateAnimeInfo(object sender, RoutedEventArgs e)
        {

        }
    }
}
