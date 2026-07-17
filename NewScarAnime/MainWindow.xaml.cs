using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MoeSoft
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public static SnackbarService GlobalSnackbarService { get; private set; }
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Acrylic, true);
            GlobalSnackbarService = new SnackbarService();
            GlobalSnackbarService.SetSnackbarPresenter(RootSnackbar);
            Instance = this;

            Start();
        }

        private void Start()
        {
            // 获取 User 文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(GetLocalAddress(), "User");

            // 如果 User 文件夹不存在，则创建
            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }

            // setting.json完整路径
            string filePath = System.IO.Path.Combine(appSpecificFolder, "setting.json");

            // 如果 setting.json不存在，则创建默认配置
            if (!File.Exists(filePath))
            {
                JObject defaultSetting = new JObject
                {
                    ["IsProxyEnabled"] = false,
                    ["ProxyAddress"] = "",
                    ["Player"] = "Default"
                };

                File.WriteAllText(
                    filePath,
                    defaultSetting.ToString()
                );
            }

            // 读取 JSON
            string json = File.ReadAllText(filePath);

            // 转换 JObject
            JObject obj = JObject.Parse(json);

            // 读取设置
            GlobalConfig.IsProxyEnabled = (bool?)obj["IsProxyEnabled"] ?? false;
            GlobalConfig.ProxyAddress = (string?)obj["ProxyAddress"] ?? "";
            GlobalConfig.Player = (string?)obj["Player"] ?? "Default";
        }

        private static string GetLocalAddress()
        {
            // 获取本地应用程序数据文件夹路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 构建应用程序专用子文件夹路径
            string appSpecificFolder = System.IO.Path.Combine(appDataFolder, "ScarAnime");

            return appSpecificFolder;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(HomePage));
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaxIcon.Text = "\uE922"; // 最大化
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaxIcon.Text = "\uE923"; // 还原
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // 双击最大化/还原
                Maximize_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        public async Task<ContentDialogResult> ShowDialogAsync(string title, string content, string primaryButtonText, string closeButtonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                PrimaryButtonAppearance = ControlAppearance.Danger, // 默认设为危险操作

                // 【关键】使用本窗口内的 ContentPresenter 作为宿主
                DialogHost = this.RootContentDialogPresenter
            };

            return await dialog.ShowAsync();
        }

        public async Task<string> ShowSpecifyLinkDialogAsync()
        {
            var inputBox = new Wpf.Ui.Controls.TextBox()
            {
                MinWidth = 260,
                PlaceholderText = "输入链接🔗"
            };

            var dialog = new ContentDialog
            {
                Title = "请输入你指定的bangumi链接",
                Content = new StackPanel
                {
                    Children =
                    {
                        inputBox
                    }
                },
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",

                // 【关键】使用本窗口内的 ContentPresenter 作为宿主
                DialogHost = this.RootContentDialogPresenter
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return inputBox.Text;
            }

            return null;
        }
    }
}
