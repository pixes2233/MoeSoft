using MoeSoft.Services;
using Newtonsoft.Json;
using System;
using System.Collections;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using static MoeSoft.HomePage;

namespace MoeSoft
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
            AnimeResult = new ObservableCollection<BangumiSearchResult>();
            this.DataContext = this;
        }

        public class BangumiSearchResult
        {
            public string title { get; set; }
            public string link { get; set; }
            public string image { get; set; }
            public BitmapImage CoverImage { get; set; }
        }

        public async Task RunBangumiSearchScraper(string title)
        {
            PythonService python = new();

            string output;

            if (GlobalConfig.IsProxyEnabled == true)
            {
                output = await python.RunPython("internet anime.py", "--proxy", GlobalConfig.ProxyAddress, title);
            }
            else
            {
                output = await python.RunPython("internet anime.py", title);
            }

            var results = JsonConvert.DeserializeObject<List<BangumiSearchResult>>(output);

            ImageService imageService = new();

            await imageService.LoadImages(results);

            foreach (var item in results)
            {
                AnimeResult.Add(item);
            }
        }

        private async void AnimeNameSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.GlobalSnackbarService.Show("System:", "正在搜索，请稍候...", ControlAppearance.Info, TimeSpan.FromSeconds(3));
                AnimeResult.Clear();
                SearchStatus.Visibility = Visibility.Visible;
                await RunBangumiSearchScraper(AnimeName.Text);
                SearchStatus.Visibility = Visibility.Collapsed;
                MainWindow.GlobalSnackbarService.Show("System:", "搜索完成 q(≧▽≦q)", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            }
        }

        private async void AddAnime(object sender, RoutedEventArgs e)
        {
            if (!(sender is Wpf.Ui.Controls.Button btn) || !(btn.DataContext is BangumiSearchResult animeItem))
                return;

            AddAnime(animeItem.link);
        }

        private async void AddAnime(string link)
        {
            try
            {
                MainWindow.GlobalSnackbarService.Show("System:", "正在添加，请耐心等待 (●'◡'●)", ControlAppearance.Info, TimeSpan.FromSeconds(3));
                await HomePage.RunBangumiScraper(link);
            }
            catch (Exception ex)
            {
                new Wpf.Ui.Controls.MessageBox { Title = "错误", Content = $"操作失败：{ex.Message}", CloseButtonText = "确定" }.ShowDialogAsync();
            }
            finally
            {
                MainWindow.GlobalSnackbarService.Show("System:", "添加完成，返回首页查看吧！q(≧▽≦q)", ControlAppearance.Info, TimeSpan.FromSeconds(3));
            }
        }

        private async void OpenLink(object sender, RoutedEventArgs e)
        {
            var animeItem = (sender as Wpf.Ui.Controls.Button)?.DataContext as BangumiSearchResult;
            if (animeItem != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = animeItem.link,
                    UseShellExecute = true
                });
            }
        }

        private void TextChange(Wpf.Ui.Controls.AutoSuggestBox sender, Wpf.Ui.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
            /// <summary>
            /// 把建议清空，防止遮挡输入框
            /// </summary>

            // 如果是用户输入，强制关闭提示列表
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = null; // 清空内容
                sender.IsSuggestionListOpen = false; // 关闭弹出的下拉提示
            }
        }

        private async void SpecifyLink(object sender, RoutedEventArgs e)
        {
            var link = await MainWindow.Instance.ShowSpecifyLinkDialogAsync();

            if (!string.IsNullOrWhiteSpace(link))
            {
                AddAnime(link);
            }
        }
    }
}
