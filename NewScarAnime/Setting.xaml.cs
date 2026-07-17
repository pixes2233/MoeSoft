using Newtonsoft.Json.Linq;
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
using System.IO;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Microsoft.Win32;

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
            PlayerName.Text = "播放器地址: " + GlobalConfig.Player;
            //try
            //{
            //    // 为你的应用程序创建专用子文件夹
            //    string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

            //    // 构建完整的文件路径
            //    // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
            //    string filePath = System.IO.Path.Combine(appSpecificFolder, "setting" + ".json");

            //    // 读取 JSON 文件
            //    string json = File.ReadAllText(filePath);

            //    // 转成 JObject
            //    JObject obj = JObject.Parse(json);

            //    ProxyToggle.IsChecked = (bool)obj["IsProxyEnabled"];
            //    ProxyAddressTextBox.Text = (string)obj["ProxyAddress"];
            //}
            //catch (Exception ex)
            //{
            //    // 显示错误消息
            //    System.Windows.MessageBox.Show($"读取 JSON 数据时出错: {ex.Message}", "读取错误");
            //}
        }

        private static string GetLocalAddress()
        {
            // 获取本地应用程序数据文件夹路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 构建应用程序专用子文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");

            return appSpecificFolder;
        }

        private void ProxyToggle_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.IsProxyEnabled = true;
            try
            {
                // 为你的应用程序创建专用子文件夹
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, "setting" + ".json");

                // 读取 JSON 文件
                string json = File.ReadAllText(filePath);

                // 转成 JObject
                JObject obj = JObject.Parse(json);

                // 修改字段
                obj["IsProxyEnabled"] = true;

                // 保存回文件
                File.WriteAllText(filePath, obj.ToString());
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"读取 JSON 数据时出错: {ex.Message}", "读取错误");
            }
        }

        private void ProxyToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.IsProxyEnabled = false;
            try
            {
                // 为你的应用程序创建专用子文件夹
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, "setting" + ".json");

                // 读取 JSON 文件
                string json = File.ReadAllText(filePath);

                // 转成 JObject
                JObject obj = JObject.Parse(json);

                // 修改字段
                obj["IsProxyEnabled"] = false;

                // 保存回文件
                File.WriteAllText(filePath, obj.ToString());
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"写入 JSON 数据时出错: {ex.Message}", "写入错误");
            }
        }

        private void ProxyAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalConfig.ProxyAddress = ProxyAddressTextBox.Text;
            try
            {
                // 为你的应用程序创建专用子文件夹
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, "setting" + ".json");

                // 读取 JSON 文件
                string json = File.ReadAllText(filePath);

                // 转成 JObject
                JObject obj = JObject.Parse(json);

                // 修改字段
                obj["ProxyAddress"] = ProxyAddressTextBox.Text.Trim();

                // 保存回文件
                File.WriteAllText(filePath, obj.ToString());
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"写入 JSON 数据时出错: {ex.Message}", "写入错误");
            }
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string proxyAddress = ProxyAddressTextBox.Text.Trim();
            ProxyConnctionStatus.Text = "正在测试连接...";
            ProxyConnctionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
            if ( await CheckProxyAsync(proxyAddress) == true )
            {
                ProxyConnctionStatus.Text = "连接成功";
                ProxyConnctionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"));
                GlobalConfig.ProxyAddress = proxyAddress;
            }
            else
            {
                ProxyConnctionStatus.Text = "连接失败";
                ProxyConnctionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
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

        private void PlayerSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "可执行文件 (*.exe)|*.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                string exePath = dialog.FileName;
                GlobalConfig.Player = exePath;
            }

            try
            {
                // 为你的应用程序创建专用子文件夹
                string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

                // 构建完整的文件路径
                // 将应用程序专用文件夹路径与你希望的文件名（默认为 "user_data.json"）结合起来。
                string filePath = System.IO.Path.Combine(appSpecificFolder, "setting" + ".json");

                // 读取 JSON 文件
                string json = File.ReadAllText(filePath);

                // 转成 JObject
                JObject obj = JObject.Parse(json);

                // 修改字段
                obj["Player"] = GlobalConfig.Player;

                // 保存回文件
                File.WriteAllText(filePath, obj.ToString());
            }
            catch (Exception ex)
            {
                // 显示错误消息
                System.Windows.MessageBox.Show($"写入 JSON 数据时出错: {ex.Message}", "写入错误");
            }
        }
    }
}
