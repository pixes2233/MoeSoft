using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
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

namespace MoeSoft
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Page
    {
        public Setting()
        {
            InitializeComponent();
            ProxyToggle.IsChecked = GlobalConfig.IsProxyEnabled;
            ProxyAddressTextBox.Text = GlobalConfig.ProxyAddress;
        }

        private void ProxyToggle_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.IsProxyEnabled = true;
        }

        private void ProxyToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.IsProxyEnabled = false;
        }

        private void ProxyAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalConfig.ProxyAddress = ProxyAddressTextBox.Text;
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string proxyAddress = ProxyAddressTextBox.Text.Trim();
            ProxyConnctionStatus.Text = "正在测试连接...";
            ProxyConnctionStatus.Foreground = new SolidColorBrush(Colors.Black);
            if ( await CheckProxyAsync(proxyAddress) == true )
            {
                ProxyConnctionStatus.Text = "连接成功";
                ProxyConnctionStatus.Foreground = new SolidColorBrush(Colors.Green);
                GlobalConfig.ProxyAddress = proxyAddress;
            }
            else
            {
                ProxyConnctionStatus.Text = "连接失败";
                ProxyConnctionStatus.Foreground = new SolidColorBrush(Colors.Red);
            }

        }

        private async Task<bool> CheckProxyAsync(string proxyUrl)
        {
            try
            {
                // 创建代理对象
                var webProxy = new WebProxy(proxyUrl)
                {
                    BypassProxyOnLocal = false
                };

                // 将代理交给 HttpClient 的处理器
                var handler = new HttpClientHandler
                {
                    Proxy = webProxy,
                    UseProxy = true,
                };

                // 创建 HttpClient
                using (var client = new HttpClient(handler))
                {
                    // 设置超时时间，避免卡太久（比如 5 秒没连上就算失败）
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // 发送一个测试请求。
                    // 推荐使用苹果的网络连通性测试接口，速度极快，且返回内容极少
                    string testUrl = "https://bangumi.tv/";

                    var response = await client.GetAsync(testUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // 如果返回了 200 OK 状态码，说明代理是通的
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // 任何网络异常（如超时、无法解析DNS、连接被拒绝）都会走到这里
                return false;
            }
        }
    }
}
