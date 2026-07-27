using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MoeSoft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 固定使用暗色主题并禁止跟随系统主题（第三个参数 false）
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Acrylic, false);

            // 粉色主题色
            SetAccentColors(Color.FromRgb(0xFF, 0x69, 0xB4),
                            Color.FromRgb(0xFF, 0xB6, 0xC1),
                            Color.FromRgb(0xFF, 0xC0, 0xCB));
        }

        /// <summary>
        /// 运行时设置全局主题色（会更新所有使用 DynamicResource 的地方）
        /// </summary>
        public static void SetAccentColors(Color primary, Color secondary, Color tertiary)
        {
            SetBrushColor("WPFUI.Theme.Accent.Primary", primary);
            SetBrushColor("WPFUI.Theme.Accent.Secondary", secondary);
            SetBrushColor("WPFUI.Theme.Accent.Tertiary", tertiary);
        }

        private static void SetBrushColor(string resourceKey, Color color)
        {
            if (Current == null) return;

            // 如果资源存在并且是 SolidColorBrush
            if (Current.Resources[resourceKey] is SolidColorBrush brush)
            {
                var newBrush = brush.Clone();
                newBrush.Color = color;
                Current.Resources[resourceKey] = newBrush;
                return;
            }

            // 如果存在但不是 SolidColorBrush，直接覆盖
            Current.Resources[resourceKey] = new SolidColorBrush(color);
        }
    }

}
