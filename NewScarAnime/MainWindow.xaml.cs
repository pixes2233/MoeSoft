using System;
using System.Collections.Generic;
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

namespace NewScarAnime
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
