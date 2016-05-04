using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DTTOOLS;

namespace wxdownload
{
    /// <summary>
    /// CutImageControl.xaml 的交互逻辑
    /// </summary>
    public partial class CutImageControl : UserControl
    {
        private ContentControl cutControl { set; get; }
        private GDITools.FitSizeTableD fit;
        private ImageBrush imageBrush { set; get; }


        public CutImageControl()
        {
            InitializeComponent();
        }

        public void LoadCutImage(BitmapImage bitmap)
        {
            try
            {
                CutBrush.Children.Clear();
                CutBrush.Background = null;
                imageBrush = new ImageBrush();
                cutControl = new ContentControl();
                cutControl.MouseWheel += cutControl_MouseWheel;
                imageBrush.Stretch = Stretch.Uniform;
                fit = GDITools.FitSize(bitmap.PixelWidth, bitmap.PixelHeight, this.ActualWidth, this.ActualHeight);
                CutBrush.Width = fit.fitw;
                CutBrush.Height = fit.fith;
                imageBrush.ImageSource = bitmap;
                CutBrush.Background = imageBrush;

                Style btn_style = (Style)this.FindResource("DesignerItemStyle");
                cutControl.Width = CutBrush.Width;
                cutControl.Height = CutBrush.Height;
                cutControl.DataContext = "CutBrush";

                Canvas.SetLeft(cutControl, 0);
                Canvas.SetTop(cutControl, 0);

                cutControl.Style = btn_style;
                CutBrush.Children.Add(cutControl);
                Selector.SetIsSelected(cutControl, true);
            }
            catch (Exception)
            {
                
            }
            
        }

        private void cutControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {


                if (cutControl.Width + 10 + Canvas.GetLeft(cutControl) < CutBrush.Width)
                {
                    cutControl.Width = cutControl.Width + 10;
                    cutControl.Height = cutControl.Height + 10;
                }

            }
            else
            {
                cutControl.Width = cutControl.Width - 10;
                cutControl.Height = cutControl.Height - 10;
            }
        }

        public ImageSource GetCutImage()
        {
           

            return SplitImage((BitmapSource)imageBrush.ImageSource, (int)(Canvas.GetLeft(cutControl) / fit.fitsize), (int)(Canvas.GetTop(cutControl) / fit.fitsize), (int)(cutControl.Width / fit.fitsize), (int)(cutControl.Height / fit.fitsize));           
        }

        public static ImageSource SplitImage(BitmapSource source, int x, int y, int tileWidth, int tileHeight)
        {
            try
            {
                // 本来对参数有校验代码，结果字数超限了
                var stride = tileWidth * ((source.Format.BitsPerPixel + 7) / 8);
                var pixelsCount = tileWidth * tileHeight;
                var tileRect = new Int32Rect(0, 0, tileWidth, tileHeight);
                var wb = new WriteableBitmap(1, 1, 1, 1, source.Format, source.Palette);
                var pixels = new int[pixelsCount];
                var copyRect = new Int32Rect(x, y, tileWidth, tileHeight);
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


    }
}
