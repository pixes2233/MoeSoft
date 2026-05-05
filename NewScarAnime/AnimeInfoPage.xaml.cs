using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using static NewScarAnime.HomePage;

namespace NewScarAnime
{
    /// <summary>
    /// AnimeInfoPage.xaml 的交互逻辑
    /// </summary>
    public partial class AnimeInfoPage : Page
    {
        private string _bangumiId;
        private string _bangumiURL;
        private string _animeTitle;

        private static string GetLocalAddress()
        {
            // 获取本地应用程序数据文件夹路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 构建应用程序专用子文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");

            return appSpecificFolder;
        }

        public class AnimeInfoViewModel
        {
            public AnimeDetails AnimeDetails { get; set; }
            public ObservableCollection<string> FileNames { get; set; }
            public ObservableCollection<string> FilePaths { get; set; }
            public ObservableCollection<bool> LastWatchStatus { get; set; }
            public ObservableCollection<FileItem> Files { get; set; }

            public AnimeInfoViewModel()
            {
                FileNames = new ObservableCollection<string>();
                FilePaths = new ObservableCollection<string>();
                LastWatchStatus = new ObservableCollection<bool>();
                Files = new ObservableCollection<FileItem>();
            }
        }

        public class FileItem
        {
            public string Name { get; set; } // 文件名
            public string Path { get; set; } // 完整路径
            public bool LastWatchStatus { get; set; } // 最近观看状态
        }

        public class AnimeDetails
        {
            public string Title { get; set; }
            public string TitleCN { get; set; }
            public string Summary { get; set; }
            public string BeginDate { get; set; }
            public string TotalEP { get; set; }
            public string Update { get; set; }
            public string Director { get; set; }
            public string Writer { get; set; }
            public string Studio { get; set; }
            public string NowEP { get; set; }
            public ImageSource AnimeCover { get; set; }
        }

        public class AnimeLocalLink
        {
            public string FilePath { get; set; } // 本地视频文件夹路径
            public string LastWatchFile { get; set; } // 最近观看的文件名
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

        public AnimeInfoPage(string bangumiId)
        {
            InitializeComponent();

            _bangumiId = bangumiId; // 将传入的ID保存到私有字段中

            // 在这里，你可以使用 _bangumiId 来加载和显示数据
            LoadAnimeData(_bangumiId);

            // 构造函数不能是 async，因此不能在此处使用 await。
            // 启动后台异步刷新但不阻塞构造函数/UI 线程。
            _ = AutoRefreshAnimeData();
        }

        private void LoadAnimeData(string id)
        {
            /// <summary>
            /// 加载アニメ信息
            /// </summary>

            string animeJson = LoadJsonOutput(id + ".json");
            if (!string.IsNullOrEmpty(animeJson))
            {
                AnimeInfo deserializedUser = JsonConvert.DeserializeObject<AnimeInfo>(animeJson);

                _bangumiURL = deserializedUser.bangumi_url;

                // 获取本地应用程序数据文件夹路径
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime", "AnimeCover");

                string bangumiId = ExtractSubjectId(deserializedUser.bangumi_url);

                string fileName = $"{bangumiId}.jpg"; // 使用提取的ID作为文件名

                string coverPath = System.IO.Path.Combine(appSpecificFolder, fileName);

                // 创建一个 BitmapImage 作为图像源
                System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();

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
                        System.Windows.MessageBox.Show($"加载图片失败: {coverPath}\n错误: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                        // （可选）设置一个占位符图像或 null
                        bitmapImage = null;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show($"封面图片未找到: {coverPath}", "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                    // （可选）设置一个占位符图像或 null
                    bitmapImage = null;
                }

                _animeTitle = deserializedUser.name;

                // 创建一个 AnimeDetails 实例并填充数据
                AnimeDetails animeData = new AnimeDetails
                {
                    Title = deserializedUser.name,
                    TitleCN = deserializedUser.name_chinese,
                    Summary = deserializedUser.summary,
                    BeginDate = "开播日期:" + deserializedUser.start_date,
                    TotalEP = "总集数:" + deserializedUser.total_episodes,
                    Update = "更新时间:" + deserializedUser.air_weekday,
                    Director = "导演:" + deserializedUser.director,
                    Writer = "编剧:" + deserializedUser.writer,
                    Studio = "制作公司:" + deserializedUser.studio,
                    NowEP = "当前更新集数:" + deserializedUser.current_episode,
                    AnimeCover = bitmapImage
                };

                List<string> fileList = GetFileListForAnime(id);
                var viewModel = new AnimeInfoViewModel();
                viewModel.AnimeDetails = animeData;
                string lastWatchFile = GetLastWatchFile(id) ?? string.Empty;
                foreach (var file in fileList)
                {
                    var fileVideoName = System.IO.Path.GetFileName(file); // 单个文件名
                    bool isLast = fileVideoName == lastWatchFile;
                    viewModel.FileNames.Add(fileVideoName);
                    viewModel.FilePaths.Add(file);
                    viewModel.LastWatchStatus.Add(isLast);
                    viewModel.Files.Add(new FileItem { Name = fileVideoName, Path = file, LastWatchStatus = true });
                }
                this.DataContext = viewModel;
            }
            else
            {
                System.Windows.MessageBox.Show("未找到相关的动漫信息。", "加载信息");
            }
        }

        private List<string> GetFileListForAnime(string id)
        {
            // 这里是你从文件系统、数据库或API中获取文件名的逻辑
            string jsonData = string.Empty; // 初始化一个空字符串，用于存储加载的数据
            try
            {
                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeLocalLink");

                // 构建完整的文件路径
                string filePath = System.IO.Path.Combine(appSpecificFolder, id + ".json");

                // 检查文件是否存在
                if (File.Exists(filePath))
                {
                    LocalLinkError.Visibility = Visibility.Collapsed;
                    // 从文件读取 JSON 数据
                    // File.ReadAllText 读取指定文件的所有文本内容，并将其作为单个字符串返回。
                    jsonData = File.ReadAllText(filePath);
                }
                else
                {
                    LocalLinkError.Visibility = Visibility.Visible;
                    return new List<string>(); // 返回空列表
                }
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"加载 JSON 数据时出错: {ex.Message}", "加载错误");
            }

            AnimeLocalLink deserializedUser = JsonConvert.DeserializeObject<AnimeLocalLink>(jsonData);

            string folderPath = $"{deserializedUser.FilePath}"; // 你的视频文件夹路径
            string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" };

            var videoFiles = Directory
                .EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => videoExtensions.Contains(System.IO.Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .ToList();

            return videoFiles;
        }

        public static string GetLastWatchFile(string fileName)
        {
            string lastWatchFile = string.Empty; // 初始化一个空字符串，用于存储加载的数据
            try
            {
                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeLocalLink");
                // 构建完整的文件路径
                string filePath = System.IO.Path.Combine(appSpecificFolder, fileName + ".json");
                // 检查文件是否存在
                if (File.Exists(filePath))
                {
                    // 从文件读取 JSON 数据
                    string jsonData = File.ReadAllText(filePath);
                    AnimeLocalLink deserializedUser = JsonConvert.DeserializeObject<AnimeLocalLink>(jsonData);
                    lastWatchFile = deserializedUser.LastWatchFile;
                }
            }
            catch (Exception ex)
            {
                // 7. 显示错误消息
                System.Windows.MessageBox.Show($"加载 JSON 数据时出错: {ex.Message}", "加载错误");
            }
            return lastWatchFile; // 返回加载到的最后观看文件名（如果未找到或出错，则返回空字符串）
        }

        public static string LoadJsonOutput(string fileName)
        {
            string jsonData = string.Empty; // 初始化一个空字符串，用于存储加载的数据
            try
            {
                // 构建应用程序专用子文件夹路径
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeInfo");

                // 构建完整的文件路径
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
                    // 如果文件不存在，显示提示信息
                    System.Windows.MessageBox.Show("未找到保存的 JSON 数据。", "加载信息");
                }
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"加载 JSON 数据时出错: {ex.Message}", "加载错误");
            }
            return jsonData; // 返回加载到的 JSON 字符串（如果未找到或出错，则返回空字符串）
        }

        private void BackToHome(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Content = new HomePage();
        }

        public static string ExtractSubjectId(string url)
        {
            /// <summary>
            /// 获取Bangumi URL 中的 Subject ID
            /// </summary>
            // 使用 Uri 类解析 URL
            Uri uri = new Uri(url);

            // 获取 URL 的路径部分
            // 对于 "https://bangumi.tv/subject/390555"，PathAndQuery 会返回 "/subject/390555"
            string path = uri.AbsolutePath;

            // 使用正则表达式提取数字
            Match match = Regex.Match(path, @"/subject/(\d+)$");

            // 检查是否匹配成功
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

        private void AddLocalLinkView(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,   // 只选文件夹
                Title = "请选择一个文件夹"
            };

            string jsonString = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                var jsonObj = new
                {
                    FilePath = folderPath,
                    LastWatchFile = "" // 初始化为一个空字符串
                };

                jsonString = JsonConvert.SerializeObject(jsonObj);
            }

            if (string.IsNullOrEmpty(jsonString))
            {
                return;
            }

            try
            {
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeLocalLink");

                // 检查该子文件夹是否存在，如果不存在则创建它。
                if (!Directory.Exists(appSpecificFolder))
                {
                    Directory.CreateDirectory(appSpecificFolder);
                }

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, _bangumiId + ".json");

                // 将 JSON 数据写入文件
                // 如果文件不存在，它会创建文件；如果文件已存在，它会覆盖现有内容。
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"保存 JSON 数据时出错: {ex.Message}", "保存错误");
            }

            LoadAnimeData(_bangumiId);
        }

        private async void RefreshAnimeData(object sender, RoutedEventArgs e)
        {
            /// <summary>
            /// 刷新アニメ信息
            /// </summary>

            MainWindow.GlobalSnackbarService.Show("System:", "正在为您刷新番剧数据", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            string animeJson = LoadJsonOutput(_bangumiId + ".json");
            AnimeInfo deserializedUser = JsonConvert.DeserializeObject<AnimeInfo>(animeJson);

            await RunBangumiScraper(deserializedUser.bangumi_url);

            LoadAnimeData(_bangumiId);
            MainWindow.GlobalSnackbarService.Show("System:", "番剧数据已刷新", ControlAppearance.Info, TimeSpan.FromSeconds(3));
        }

        private async Task AutoRefreshAnimeData()
        {
            /// <summary>
            /// Auto刷新アニメ信息
            /// </summary>

            string animeJson = LoadJsonOutput(_bangumiId + ".json");
            AnimeInfo deserializedUser = JsonConvert.DeserializeObject<AnimeInfo>(animeJson);

            await RunBangumiScraper(deserializedUser.bangumi_url);

            LoadAnimeData(_bangumiId);
            AutoProgress.Visibility = Visibility.Collapsed;
            AutoLoadText.Visibility = Visibility.Collapsed;
        }

        private void OpenBangumiURL(object sender, RoutedEventArgs e)
        {
            MainWindow.GlobalSnackbarService.Show("System:", "已打开 Bangumi 页面", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            Process.Start(new ProcessStartInfo
            {
                FileName = _bangumiURL,
                UseShellExecute = true // 必须为 true，否则不会用默认浏览器
            });
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            string fileNameOnly = "";

            if (sender is Wpf.Ui.Controls.CardAction card)
            {
                // 获取绑定的数据上下文，也就是 FileItem
                if (card.DataContext is FileItem file)
                {
                    fileNameOnly = System.IO.Path.GetFileName(file.Path);

                    // 如果你要直接打开文件，可以在这里调用
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = file.Path,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"打开文件失败：{ex.Message}");
                    }
                }
            }

            e.Handled = true;

            try
            {
                // 为你的应用程序创建专用子文件夹
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "AnimeLocalLink");

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, _bangumiId + ".json");

                // 读取 JSON 文件
                string json = File.ReadAllText(filePath);

                // 转成 JObject
                JObject obj = JObject.Parse(json);

                // 修改字段
                obj["LastWatchFile"] = fileNameOnly;

                // 保存回文件
                File.WriteAllText(filePath, obj.ToString());
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"保存 JSON 数据时出错: {ex.Message}", "保存错误");
            }

            LoadAnimeData(_bangumiId);
        }

        private async void DeleteAnime(object sender, RoutedEventArgs e)
        {
            var result = await MainWindow.Instance.ShowDialogAsync(
                "确认删除",
                "你确定要永久删除这部番剧吗？此操作无法撤销。",
                "确认删除",
                "取消"
            );

            if (result == ContentDialogResult.Primary)
            {
                // 用户点击了“确认删除”
                PerformDeleteAction();
            }
            else
            {

            }
        }

        private void PerformDeleteAction()
        {
            MainWindow.GlobalSnackbarService.Show("System:", $"正在删除「{_animeTitle}」", ControlAppearance.Info, TimeSpan.FromSeconds(3));

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");
            string animeCoverFolder = System.IO.Path.Combine(appSpecificFolder, "AnimeCover");
            string animeInfoFolder = System.IO.Path.Combine(appSpecificFolder, "AnimeInfo");
            string animeLocalLinkFolder = System.IO.Path.Combine(appSpecificFolder, "AnimeLocalLink");

            string infoFilePath = System.IO.Path.Combine(animeInfoFolder, $"{_bangumiId}.json");
            string linkFilePath = System.IO.Path.Combine(animeLocalLinkFolder, $"{_bangumiId}.json");
            string coverFilePath = System.IO.Path.Combine(animeCoverFolder, $"{_bangumiId}.jpg");

            // --- 3. 依次删除这三个文件 ---
            SafeDeleteFile(infoFilePath);
            SafeDeleteFile(linkFilePath);
            SafeDeleteFile(coverFilePath);

            this.NavigationService.Content = new HomePage();
        }

        private void SafeDeleteFile(string filePath)
        {
            // 首先检查文件是否存在
            if (!System.IO.File.Exists(filePath))
            {
                MainWindow.GlobalSnackbarService.Show("System:", $"文件不存在，跳过删除: 「{_animeTitle}」", ControlAppearance.Info, TimeSpan.FromSeconds(3));
                return; // 文件不存在，直接返回
            }

            try
            {
                // 文件存在，执行删除
                System.IO.File.Delete(filePath);
                MainWindow.GlobalSnackbarService.Show("System:", $"成功删除文件: 「{_animeTitle}」", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                // 捕获可能发生的异常（如权限不足、文件被占用等）
                MainWindow.GlobalSnackbarService.Show("System:", $"删除文件失败: 「{_animeTitle}」。错误: {ex.Message}", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            }
        }
    }
}
