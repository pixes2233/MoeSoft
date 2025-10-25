using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static NewScarAnime.HomePage;

namespace NewScarAnime
{
    /// <summary>
    /// BangumiSearch.xaml 的交互逻辑
    /// </summary>
    public partial class BangumiSearch : Page
    {
        public ObservableCollection<BangumiSearchResult> AnimeResult { get; set; }

        public BangumiSearch()
        {
            InitializeComponent();
            this.DataContext = this;
            AnimeResult = new ObservableCollection<BangumiSearchResult>();
        }

        public class BangumiSearchResult
        {
            public string title { get; set; }
            public string link { get; set; }
            public string image { get; set; }
        }

        public async Task RunBangumiSearchScraper(string title)
        {
            // 指定要启动的exe文件路径
            var exePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Scrap", "BangumiSearchScrap.exe");

            // 要传递给exe的参数
            string argument = title; // 这是你要作为命令行参数传递的字符串

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

                        if (!string.IsNullOrWhiteSpace(output))
                        {
                            var results = JsonConvert.DeserializeObject<List<BangumiSearchResult>>(output);

                            foreach (var item in results)
                            {
                                if (string.IsNullOrWhiteSpace(item.image))
                                {
                                    item.image = "pack://application:,,,/Icon/DontFindImage.png";
                                }

                                AnimeResult.Add(new BangumiSearchResult
                                {
                                    title = item.title,
                                    link = item.link,
                                    image = item.image
                                });
                            }
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

        private async void AnimeNameSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AnimeResult.Clear();
                SearchStatus.Visibility = Visibility.Visible;
                await RunBangumiSearchScraper(AnimeName.Text);
                SearchStatus.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddAnime(object sender, RoutedEventArgs e)
        {
            var animeItem = (sender as Wpf.Ui.Controls.Button)?.DataContext as BangumiSearchResult;
            if (animeItem != null)
            {
                await HomePage.RunBangumiScraper(animeItem.link);
            }
        }

    }
}
