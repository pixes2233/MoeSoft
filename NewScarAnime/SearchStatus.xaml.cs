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

namespace NewScarAnime
{
    /// <summary>
    /// SearchStatus.xaml 的交互逻辑
    /// </summary>
    public partial class SearchStatus : Page
    {
        public SearchStatus(bool status)
        {
            InitializeComponent();
            if(status == false)
            {
                StatusText.Text = "搜索完成！";
                Status.Visibility = Visibility.Hidden;
            }
        }
    }
}
