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
using System.Windows.Shapes;
using System.IO;

namespace wxdownload
{
    /// <summary>
    /// HelpWindows.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindows : Window
    {
        private List<string> imageList = new List<string>();
        private int pageIndex = 0;
        public HelpWindows()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            var smPath = Directory.GetCurrentDirectory() + "\\sm\\";
            if(!Directory.Exists(smPath)) return;

            imageList = Directory.GetFiles(smPath, "*.jpg",
                SearchOption.TopDirectoryOnly).ToList();

           imageList=  imageList.OrderBy(x => x).ToList();

            sImage.Source = DTTOOLS.GDITools.LoadBitmapImage(imageList[0], 0, 0, false);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (pageIndex == 0)
            {
                MessageBox.Show("已经是第一页");
                return;
            }

            pageIndex = pageIndex - 1;

            sImage.Source = DTTOOLS.GDITools.LoadBitmapImage(imageList[pageIndex], 0, 0, false);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (pageIndex == imageList.Count - 1)
            {
                MessageBox.Show("已经是最后一页");
                return;
            }

            pageIndex = pageIndex + 1;

            sImage.Source = DTTOOLS.GDITools.LoadBitmapImage(imageList[pageIndex], 0, 0, false);
        }
    }
}
