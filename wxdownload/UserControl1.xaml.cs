using System;
using System.Collections.Generic;
using System.Drawing.Design;
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
using System.Drawing;
using System.IO;
using DT.PicPrintBll;
using System.Drawing.Imaging;
namespace wxdownload
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public  Bitmap orgImage { set; get; }

        public UserControl1()
        {
            InitializeComponent();
        }

        Thickness rg;
        double x = -1, y = -1;
        double px = 0, py = 0;
        double maxX = 0, maxY = 0;
        double initWidth=0, initHeight=0;
        double inix=0, iniy=0;
        
   
        public void ImageInit()
        {
            image1.Source = null;
        }
        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(null);
            x = p.X;
            y = p.Y;
        }

        private void image1_MouseMove(object sender, MouseEventArgs e)
        {
            rg = rectangle1.Margin;
            System.Windows.Point p = e.GetPosition(null);
            if (x != -1)
            {
                px = p.X - x;
                py = p.Y - y;
                if (rg.Left + px > maxX)
                    px = maxX - rg.Left;
                else if (rg.Left + px < 0)
                    px = 0 - rg.Left;
                if (rg.Top + py > maxY)
                    py = maxY - rg.Top;
                else if (rg.Top + py < 0)
                    py = 0 - rg.Top;
                rectangle1.Margin = new Thickness(rg.Left + px, rg.Top + py, 0, 0);
                x = p.X;
                y = p.Y;
            }
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
        {
            x = -1;
            y = -1;
        }

        private void image1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double tmp = slider1.Value;
            if (e.Delta > 0)
            {
                tmp += 10;
                if (tmp > slider1.Maximum)
                {
                    tmp = slider1.Maximum;
                }
            }
            else
            {
                tmp -= 10;
                if (tmp < 0)
                {
                    tmp = 0;
                }
            }
            slider1.Value = tmp;
        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            x = -1;
            y = -1;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rg = rectangle1.Margin;
            double Width, Height, x, y;
            x = rg.Left;
            y = rg.Top;
            if (image1.ActualHeight > initHeight)
            {
                Width = slider1.Value;
                Height = slider1.Value *iniy / inix;
            }
            else
            {
                Height = slider1.Value;
                Width = slider1.Value * inix / iniy;
            }
            if (x + Width > image1.ActualWidth)
                x = image1.ActualWidth - Width;
            if (y + Height > image1.ActualHeight)
                y = image1.ActualHeight - Height;
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            rectangle1.Margin = new Thickness(x, y, 0, 0);
            //rg.Rect = new Rect(x, y, Width, Height);
            rectangle1.Width = Width;
            rectangle1.Height = Height;
            maxX = image1.ActualWidth - Width;
            maxY = image1.ActualHeight - Height;
        }
        public static ImageSource SplitImage(BitmapSource source,int x,int y, int tileWidth, int tileHeight)
        {
            try
            {
                if ((source.PixelWidth > source.PixelHeight && tileWidth < tileHeight) || (source.PixelWidth < source.PixelHeight && tileWidth > tileHeight))
                {
                    int wx, hy;
                    wx = tileHeight;
                    hy = tileWidth;
                    tileHeight = hy;
                    tileWidth = wx;
                }
             
                // 本来对参数有校验代码，结果字数超限了
                var stride = tileWidth * ((source.Format.BitsPerPixel + 7) / 8);
                
                var pixelsCount = tileWidth * tileHeight;
                var tileRect = new Int32Rect(0, 0, tileWidth, tileHeight);         
                        var wb = new WriteableBitmap(1, 1, 1, 1, source.Format, source.Palette);
                        var pixels = new int[pixelsCount];
                        var copyRect = new Int32Rect(x,y, tileWidth, tileHeight);
                        source.CopyPixels(copyRect, pixels, stride, 0);
                        wb = new WriteableBitmap(
                            tileWidth,
                            tileHeight,
                            source.DpiX,
                            source.DpiY,
                            source.Format,
                            source.Palette);
                        wb.Lock();
                        wb.WritePixels(tileRect, pixels, stride, 0);
                        wb.Unlock();
                return wb;


            }
            catch
            {
                return null;
            }

        }

        public ImageSource ImageGet(int w, int h)
        {         
            double x = w / image1.ActualWidth;
            double y = h / image1.ActualHeight;
            System.Drawing.Rectangle rec = new System.Drawing.Rectangle();
            rec.Width = (int)(rectangle1.Width * x);
            rec.Height = (int)(rectangle1.Height * y);
            rec.X = (int)(rectangle1.Margin.Left * x);
            rec.Y = (int)(rectangle1.Margin.Top * y);
            var  bit = SplitImage((BitmapImage)image1.Source, rec.X, rec.Y, rec.Width, rec.Height);
            slider1.IsEnabled = false;
            rectangle1.Visibility = System.Windows.Visibility.Hidden;
            rectangle1.Margin = new Thickness(0, 0, 0, 0);
            return bit;


        }

        internal void ImageSet(BitmapImage a, double w ,double h)
        {
            image1.Source = a;
            fq_WpfApplication.DoEvents();
            if (image1.ActualHeight == 0)
            {
                rectangle1.Width = w / 4;
                rectangle1.Height = h / 4;
            }
            else
            {
                if (slider1.IsEnabled == false)
                {

                    inix = a.Width;
                    iniy = a.Height;


                    initWidth = image1.ActualHeight * inix / iniy;
                    initHeight = image1.ActualWidth * iniy / inix;
                    if (image1.ActualHeight > initHeight)
                    {
                        slider1.Maximum = image1.ActualWidth;
                        slider1.Value = image1.ActualWidth;
                        slider1.Minimum = image1.ActualWidth * 60 / 100;
                        rectangle1.Margin = new Thickness(0, (image1.ActualHeight - initHeight) / 2, 0, 0);
                        rectangle1.Width = image1.ActualWidth;
                        rectangle1.Height = initHeight;
                    }
                    else
                    {
                        slider1.Maximum = image1.ActualHeight;
                        slider1.Value = image1.ActualHeight;
                        slider1.Minimum = image1.ActualHeight * 60 / 100;
                        rectangle1.Margin = new Thickness((image1.ActualWidth - initWidth) / 2, 0, 0, 0);
                        rectangle1.Width = initWidth;
                        rectangle1.Height = image1.ActualHeight;
                    }
                }
                slider1.IsEnabled = true;
                rectangle1.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
