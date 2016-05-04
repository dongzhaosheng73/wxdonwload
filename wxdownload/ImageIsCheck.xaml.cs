using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wxdownload
{
    /// <summary>
    /// ImageIsCheck.xaml 的交互逻辑
    /// </summary>
    public partial class ImageIsCheck : UserControl
    {
        public delegate void sIsCheck(object control);
        public event  sIsCheck Event_sIsCheck ;

    
        public ImageIsCheck()
        {
            InitializeComponent();
        }

        private void img_Phote_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Event_sIsCheck != null) Event_sIsCheck(this.grid_photo);
        }
    }
}
