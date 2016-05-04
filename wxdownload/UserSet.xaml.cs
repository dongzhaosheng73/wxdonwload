using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading;
using System.Windows.Media;
using DTTOOLS.IOHepler;
using DTTOOLS.Tools;
using Microsoft.Win32;

namespace wxdownload
{
    /// <summary>
    /// UserSet.xaml 的交互逻辑
    /// </summary>
    public partial class UserSet
    {
        public UserSet()
        {
            InitializeComponent();
        }

        public delegate void Readset();

        public event Readset EventReadset;
  
        private int PhotoNum { set; get; }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            var rPath = Directory.GetCurrentDirectory() + "\\Set.ini";

            DTTOOLS.DTIni.Ini(rPath);

            var wErrorlog = new Errorlog(Directory.GetCurrentDirectory() + "\\log\\");

            wErrorlog.WriteError("RsetPath=" + rPath);

            var print = DTTOOLS.DTIni.ReadValue("Print", "PrintName");

            wErrorlog.WriteError("RsetPrint=" + print);

            comb_print.ItemsSource =   DTTOOLS.Print.PrintTools.GetLocalPrinters();
            
             if (print == "" )
             {
                 comb_print.SelectedIndex = 0;
             }
             else
             {
                 comb_print.SelectedValue = print;
             }

            if (comb_print.Text == "") return;

            var printpaper = DTTOOLS.DTIni.ReadValue("Print", "PaperSize");

            wErrorlog.WriteError("RsetPage=" + printpaper);

            var pd = new System.Drawing.Printing.PrintDocument {PrinterSettings = {PrinterName = comb_print.Text}};

            comb_size.ItemsSource = DTTOOLS.Print.PrintTools.GetPrintPageType(pd);

            comb_size.DisplayMemberPath = "PaperName";

            if (printpaper == "")
            {
                comb_size.SelectedIndex = 0;
            }
            else
            {
                comb_size.Text = printpaper;
            }

            text_ExePath.Text = DTTOOLS.DTIni.ReadValue("Other", "PhotoExe");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DTTOOLS.DTIni.Write("Print", "PrintName", comb_print.Text);

            DTTOOLS.DTIni.Write("Print", "PaperSize", comb_size.Text);

            DTTOOLS.DTIni.Write("Other", "PhotoExe", text_ExePath.Text);

            if (EventReadset != null) EventReadset();

            Close();
        }

        private void comb_print_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pd = new System.Drawing.Printing.PrintDocument
            {
                PrinterSettings = {PrinterName = comb_print.SelectedItem.ToString()}
            };

            comb_size.ItemsSource = DTTOOLS.Print.PrintTools.GetPrintPageType(pd);

            comb_size.DisplayMemberPath = "PaperName";

            comb_size.SelectedIndex = 0;
        }

        private void but_PhotoExe_Click(object sender, RoutedEventArgs e)
        {          
                var openfile = new OpenFileDialog { Filter = "EXE|*.exe" };
                openfile.ShowDialog();

                if (!String.IsNullOrWhiteSpace(openfile.FileName))
                {
                    text_ExePath.Text = openfile.FileName;
                }      
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dir = Directory.GetCurrentDirectory() + "\\photo\\" + DateTime.Now.ToString("yyMMdd") + "\\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "\\photo\\" + DateTime.Now.ToString("yyMMdd") + "\\");
            
           
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            LabelState.Content = "开始计算缓存目录容量";
         
            PhotoNum = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\photo\\", "*.*",
                SearchOption.AllDirectories).Length;
            if (PhotoNum != 0)
            {
                LabelState.Content = string.Format("发现{0}个缓存文件开始清理",
                    PhotoNum);

                Thread deleteFile = new Thread(ThreadDelete);
                deleteFile.IsBackground = true;
                deleteFile.Start();
            }
            else
            {
                LabelState.Content = "暂未发现缓存文件";
            }
        }

        private void ThreadDelete()
        {
            IOHepler  ioHepler = new IOHepler();
            ioHepler.Event_DeletnIndex += ioHepler_Event_DeletnIndex;
            ioHepler.DeleteDirectorys(Directory.GetCurrentDirectory() + "\\photo\\");
        }

        private void ioHepler_Event_DeletnIndex(int index)
        {
           LabelState. Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                new Action(delegate
                {
                    LabelState.Content = string.Format("处理进度{0}/{1}", index, PhotoNum);
                    if (index == PhotoNum) LabelState.Content = "清理完毕！";
                }));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            HelpWindows helpw = new HelpWindows();
            helpw.Show();
        }

   
    }
}
