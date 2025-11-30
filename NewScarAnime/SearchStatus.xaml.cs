using System;
using System.Collections.Generic;
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
using static NewScarAnime.BangumiSearch;
using static NewScarAnime.HomePage;

namespace NewScarAnime
{
    /// <summary>
    /// SearchStatus.xaml 的交互逻辑
    /// </summary>
    public partial class SearchStatus : Page
    {
        public SearchStatus(string link)
        {
            InitializeComponent();
            AddAnime(link);
        }

        private async void AddAnime(string link)
        {
            var mainWindow = Application.Current?.MainWindow as Window;

            try
            {
                // 禁用主窗口，阻止用户交互
                mainWindow.IsEnabled = false;

                // 等待爬虫完成（异步，不阻塞 UI 线程）
                await HomePage.RunBangumiScraper(link);

                mainWindow.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                mainWindow.IsEnabled = true;
            }
            StatusText.Text = "操作完成！请返回首页查看结果。";
            Status.Visibility = Visibility.Hidden;
        }
    }
}
