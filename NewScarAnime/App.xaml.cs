using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui;

namespace NewScarAnime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 可选：启动时设置（保留你在 XAML 中的默认颜色）
            SetAccentColors(Color.FromRgb(0x00, 0x7A, 0xCC),
                            Color.FromRgb(0x1E, 0x90, 0xFF),
                            Color.FromRgb(0x46, 0x82, 0xB4));
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
