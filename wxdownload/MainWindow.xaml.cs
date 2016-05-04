using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CsharpHttpHelper;
using DT.PicPrintBll;
using DTTOOLS;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using DTTOOLS.ExpandClass;
using DTTOOLS.Print;
using DTTOOLS.Tools;
using Microsoft.Win32;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace wxdownload
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public string PrintName { set; get; }

        private string PaperSize { set; get; }

        private string PhotoExe { set; get; }

        public string PrintFile { set; get; }

        public string SavePhoto { set; get; }

        public string SaveUserHeadPhoto { set; get; }

        public string UserUrlAPI { set; get; }

        public string UserMessageUrlAPI { set; get; }

        public string ErrorPath { set; get; }

        public UserData Userdata = new UserData();

        private List<DataList> _dList = new List<DataList>();

        private MemoryStream _priMemoryStream = new MemoryStream();

    

        public int Angle { set; get; }

        /// <summary>
        /// 选中框
        /// </summary>
        private readonly Rectangle _isCheckRectangle = new Rectangle();

        /// <summary>
        /// 用户列表页数
        /// </summary>
        public int PageNum = 0;

        /// <summary>
        /// 用户列表索引
        /// </summary>
        public int PageIndex = 0;

        /// <summary>
        /// 消息列表页数
        /// </summary>
        public int MessgPageNum = 0;

        /// <summary>
        /// 消息列表页数
        /// </summary>
        public int MessgPageIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void but_refresh_Click(object sender, RoutedEventArgs e)
        {
            var rest = new Thread(RefreshUserList) {IsBackground = true};
            rest.Start();

        }

        private void RefreshUserList()
        {
            try
            {
                PageIndex = 0;

                var http = new HttpHelper();

                var item = new HttpItem
                {
                    URL = UserUrlAPI,

                    Method = "GET",

                    ContentType = "text/html",
                };

                //请求的返回值对象
                HttpResult result = null;
                if (ListUserlist == null) return;
                ListUserlist.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new Action(delegate
                    {
                        var ld = new loading();
                        ld.Show();
                        Thread.Sleep(500);
                        result = http.GetHtml(item);
                        ld.Close();
                    }));
                //获取请请求的Html
                var html = result.Html;

                Userdata = (UserData) HttpHelper.JsonToObject<UserData>(html);

                if (Userdata.data.OpenIds.Count > 0)
                {

                    PageNum = (int) Math.Ceiling((double) Userdata.data.OpenIds.Count/7);

                    ListUserlist.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                        new Action(delegate
                        {
                            ListUserlist.ItemsSource = UserListFull(7);

                            lab_page.Content = PageIndex + 1 + "/" + PageNum;

                            ListUserlist.SelectedIndex = 0;
                        }));              
                }
                else
                {
                    MessageBox.Show("数据拉取失败！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"网络异常！");
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        /// <summary>
        /// 填充用户列表
        /// </summary>
        /// <returns></returns>
        private IEnumerable<User_OpenIds> UserListFull(int num)
        {
            var userItem = new System.Collections.ObjectModel.ObservableCollection<User_OpenIds>();

            for (var i = 0; i < num; i++)
            {
                if (PageIndex * num + i >= Userdata.data.OpenIds.Count) break;
                string headimage;
                if (!File.Exists(SaveUserHeadPhoto + Userdata.data.OpenIds[(PageIndex)*num + i].OpenId + ".jpg"))
                {
                    headimage = GetHeadImage(Userdata.data.OpenIds[(PageIndex)*num + i].HeadImgUrl,
                        Userdata.data.OpenIds[PageIndex*num + i].OpenId);
                }
                else
                {
                    headimage = SaveUserHeadPhoto + Userdata.data.OpenIds[(PageIndex)*num + i].OpenId + ".jpg";
                }
                var username = Userdata.data.OpenIds[(PageIndex)*num + i].NickName;

                if (username == "") username = "未知用户";

                userItem.Add(new User_OpenIds
                {
                    HeadImgUrl = headimage,
                    NickName = username,
                    OpenId = Userdata.data.OpenIds[(PageIndex)*num + i].OpenId
                });
            }
            return userItem;
        }

        private string GetHeadImage(string url, string id)
        {
            var uerphoto = SaveUserHeadPhoto + id + ".jpg";
            var http = new HttpHelper();
            var itemm = new HttpItem {URL = url, Method = "GET", ResultType = CsharpHttpHelper.Enum.ResultType.Byte};
            var rt = http.GetHtml(itemm);
            if (rt.ResultByte != null)
            {
                try
                {
                    HttpHelper.GetImage(rt.ResultByte).Save(uerphoto, ImageFormat.Jpeg);
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
            return uerphoto;
        }


        private void but_Previous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PageNum == 0) return;
                if (PageIndex < 0) return;
                PageIndex--;
                if (PageIndex < 0)
                {
                    PageIndex = 0;
                }
                ListUserlist.ItemsSource = UserListFull(7);

                lab_page.Content = PageIndex + 1 + "/" + PageNum;

            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void but_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PageNum == 0) return;

                if (PageIndex == PageNum) return;

                PageIndex++;

                if (PageIndex == PageNum)
                {
                    PageIndex = PageIndex - 1;
                    return;
                }
                ListUserlist.ItemsSource = UserListFull(7);

                lab_page.Content = PageIndex + 1 + "/" + PageNum;

            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void list_userlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Filling()
        {
            try
            {
                _dList.Clear();
                MessgPageNum = 0;
                MessgPageIndex = 0;
                User_OpenIds useritem = null;
                ListUserlist.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new Action(delegate {
                                            useritem = (User_OpenIds) ListUserlist.SelectedItem;
                    }));

                var http = new HttpHelper();

                var item = new HttpItem
                {
                    URL =
                        @"http://easy.51diantu.com/Pay/PhotoMpLoad/GetOpenIdDetail?appid=gh_e3c3f33f35fb&openId=" +
                        useritem.OpenId,

                    Method = "GET",

                    ContentType = "text/html",
                };
                //请求的返回值对象      
                var result = http.GetHtml(item);

                sp_messgbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new Action(delegate
                    {
                        try
                        {
                            //获取请请求的Html
                            var html = result.Html;

                            var userImage = (UserImageData) HttpHelper.JsonToObject<UserImageData>(html);

                            foreach (var img in userImage.data.Images)
                            {
                                _dList.Add(new DataList
                                {
                                    Datatime = DateTime.Parse(img.Time),
                                    Imageurl = img.ImageUrl,
                                    Text = "",
                                    Id = img.MsgId
                                });
                            }
                            foreach (var tx in userImage.data.Texts)
                            {
                                _dList.Add(new DataList
                                {
                                    Datatime = DateTime.Parse(tx.Time),
                                    Imageurl = "",
                                    Text = tx.Content,
                                    Id = tx.MsgId
                                });
                            }

                            _dList = _dList.OrderByDescending(x => x.Datatime).ToList();

                            sp_messgbox.Children.Clear();

                            if (_dList.Count > 0)
                            {
                                MessgPageNum = (int)Math.Ceiling((double) _dList.Count/10);

                                sp_messgbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                                    new Action(delegate
                                    {
                                        var ld = new loading();
                                        ld.Show();
                                        Thread.Sleep(500);
                                        DataFull();
                                        ld.Close();
                                    }));

                                lab_messgpage.Content = MessgPageIndex + 1 + "/" + MessgPageNum;
                            }
                            else
                            {
                                MessageBox.Show("数据拉取失败！");

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(@"网络异常！");
                            var er = new Errorlog(ErrorPath);
                            er.WriteError(ex);
                        }

                    }));
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void but_MessgPrevious_Click(object sender, RoutedEventArgs e)
        {
            sp_messgbox.Children.Clear();
            var nextpage = new Thread(MessgNextPage) {IsBackground = true};
            nextpage.Start("ago");
        }

        private void but_MessgNext_Click(object sender, RoutedEventArgs e)
        {
            sp_messgbox.Children.Clear();
            var rearpage = new Thread(MessgNextPage) {IsBackground = true};
            rearpage.Start("Rear");

        }

        /// <summary>
        /// 消息栏翻页
        /// </summary>
        /// <param name="state">翻页状态</param>
        private void MessgNextPage(object state)
        {
            try
            {
                sp_messgbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new Action(delegate
                    {
                        if (MessgPageNum == 0) return;

                        if (MessgPageIndex > MessgPageNum) return;

                        if (state.ToString() == "Rear")
                        {
                            MessgPageIndex = MessgPageIndex + 1;

                            if (MessgPageIndex == MessgPageNum)
                            {
                                MessgPageIndex = MessgPageIndex - 1;
                            }
                          
                        }
                        else
                        {
                            if (MessgPageIndex < 0) return;

                            MessgPageIndex = MessgPageIndex - 1;

                            if (MessgPageIndex < 0)
                            {
                                MessgPageIndex = 0;
                            }
                        }
                        sp_messgbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                            new Action(delegate
                            {
                                var ld = new loading();
                                ld.Show();
                                Thread.Sleep(500);
                                DataFull();
                                ld.Close();
                            }));

                        lab_messgpage.Content = MessgPageIndex  + 1 + "/" + MessgPageNum;
                    }));
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        /// <summary>
        /// 填充消息栏数据
        /// </summary>
        private void DataFull()
        {
            var http = new HttpHelper();

            // ReSharper disable once ObjectCreationAsStatement
            new loading();

            for (var i = 0; i < 10; i++)
            {
                if (MessgPageIndex*10 + i >= _dList.Count) continue;
                if (_dList[MessgPageIndex*10 + i].Imageurl != "")
                {

                    var itemm = new HttpItem
                    {
                        URL = _dList[MessgPageIndex*10 + i].Imageurl,
                        Method = "GET",
                        ResultType = CsharpHttpHelper.Enum.ResultType.Byte
                    };

                    var img = new ImageIsCheck();
                    img.Event_sIsCheck += img_Event_sIsCheck;
                    img.grid_photo.DataContext = _dList[MessgPageIndex*10 + i].Id;

                    img.Width = 400;
                    img.Height = 400;

                    var savePath = SavePhoto + _dList[MessgPageIndex*10 + i].Id + ".jpg";


                    if (!File.Exists(savePath))
                    {
                        
                        HttpResult rt = http.GetHtml(itemm);
                        if (rt.ResultByte != null)
                        {
                            try
                            {
                                using (  var m = new MemoryStream())
                                {
                                    HttpHelper.GetImage(rt.ResultByte).Save(m, ImageFormat.Jpeg);
                                    m.Seek(0, SeekOrigin.Begin);
                                    if (!File.Exists(savePath))
                                    {
                                        var savebitmap = new Bitmap(m);
                                        savebitmap.Save(savePath);
                                        savebitmap.Dispose();
                                    }
                                }

                                MemoryStream rm = new MemoryStream(File.ReadAllBytes(savePath));
                                BitmapImage bit = new BitmapImage();
                                bit.BeginInit();
                                bit.StreamSource = rm;
                                bit.EndInit();

                                img.img_Phote.Source = bit;
                                img.lab_timedata.Content = string.Format("{0:u}", _dList[MessgPageIndex*10 + i].Datatime);
                                GDITools.FlushMemory();
                                sp_messgbox.Children.Add(img);

                            }
                            catch (Exception ex)
                            {
                                var er = new Errorlog(ErrorPath);
                                er.WriteError(ex);
                            }
                        }
                    }
                    else
                    {
                        var filebytes = File.ReadAllBytes(savePath);
                        var bit = new BitmapImage {CacheOption = BitmapCacheOption.None};
                        bit.BeginInit();
                        bit.StreamSource = new MemoryStream(filebytes);
                        bit.EndInit();
                        img.img_Phote.Source = bit;
                        img.lab_timedata.Content = string.Format("{0:u}", _dList[MessgPageIndex*10 + i].Datatime);
                        GDITools.FlushMemory();
                        sp_messgbox.Children.Add(img);
                    }
                }
                if (_dList[MessgPageIndex*10 + i].Text == "") continue;
                var lb = new MessageLable
                {
                    Width = 400,
                    lab_datatime = {Content = string.Format("{0:u}", _dList[MessgPageIndex*10 + i].Datatime)},
                    textblock_usermessage = {Text = _dList[MessgPageIndex*10 + i].Text},
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                sp_messgbox.Children.Add(lb);
            }
        }

        public void img_Event_sIsCheck(object control)
        {
            try
            {
                if (!ReferenceEquals(but_cut.Content, "确认"))
                {
                    var g = (Grid) _isCheckRectangle.Parent;

                    if (g != null) g.Children.Remove(_isCheckRectangle);

                    var color = ColorTranslator.FromHtml("#c9616d");

                    _isCheckRectangle.StrokeThickness = 5;

                    _isCheckRectangle.Margin = new Thickness(0, 0, 0, 0);

                    _isCheckRectangle.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));

                    Grid.SetRow(_isCheckRectangle, 1);

                    var grid = control as Grid;
                    if (grid != null) grid.Children.Add(_isCheckRectangle);

                    var grid1 = control as Grid;
                    if (grid1 == null) return;
                    var readPath = SavePhoto + grid1.DataContext + ".jpg";
                    if (!File.Exists(readPath)) return;
                    byte[] readBytes = File.ReadAllBytes(readPath);
                    _priMemoryStream = new MemoryStream(readBytes);                
                    _priMemoryStream.Seek(0, SeekOrigin.Begin);
                    var bit = new BitmapImage();
                    bit.BeginInit();
                    bit.StreamSource = _priMemoryStream;
                    bit.EndInit();
                    ImageShowImage.Source = bit;
                    PrintFile = readPath;
                    GDITools.FlushMemory();
                   
                }
                else
                {
                    MessageBox.Show("请先确认剪裁！");

                }
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {              
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

                SavePhoto = Directory.GetCurrentDirectory() + "\\photo\\" + DateTime.Now.ToString("yyMMdd") + "\\";

                SaveUserHeadPhoto = Directory.GetCurrentDirectory() + "\\userheadphoto\\";

                ErrorPath = Directory.GetCurrentDirectory() + "\\error\\";

                if (!Directory.Exists(SavePhoto)) Directory.CreateDirectory(SavePhoto);

                if (!Directory.Exists(SaveUserHeadPhoto)) Directory.CreateDirectory(SaveUserHeadPhoto);

                if (!Directory.Exists(ErrorPath)) Directory.CreateDirectory(ErrorPath);

                DTIni.Ini(Directory.GetCurrentDirectory() + "\\Set.ini");

                PrintName = DTIni.ReadValue("Print", "PrintName");

                if (!DTTOOLS.Print.PrintTools.GetLocalPrinters().Contains(PrintName))
                {
                    MessageBox.Show("无法找到配置打印机请重新设置打印机");
                    var set = new UserSet();
                    set.EventReadset += set_EventReadset;
                    set.Show();
                }

                PaperSize = DTIni.ReadValue("Print", "PaperSize");

                UserUrlAPI = DTIni.ReadValue("NetValue", "UserUrlAPI");

                PhotoExe = DTIni.ReadValue("Other", "PhotoExe");

                if (String.IsNullOrWhiteSpace(UserUrlAPI))
                {
   
                    UserUrlAPI = @"http://easy.51diantu.com/Pay/PhotoMpLoad/GetOpenIdList?appid=gh_e3c3f33f35fb&sceneid=9";
                    DTIni.Write("NetValue", "UserUrlAPI", UserUrlAPI);
                }

                UserMessageUrlAPI = DTIni.ReadValue("NetValue", "UserMessageUrlAPI");
                if (String.IsNullOrWhiteSpace(UserMessageUrlAPI))
                {
                    UserMessageUrlAPI =
                        @"http://easy.51diantu.com/Pay/PhotoMpLoad/GetOpenIdDetail?appid=gh_e3c3f33f35fb&sceneid=9";
                    DTIni.Write("NetValue", "UserMessageUrlAPI", UserMessageUrlAPI);
                }

               
 
            }
            catch (Exception ex)
            {

                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

       
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {

                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,

                FileShare.None);

                inUse = false;
            }
            catch
            {

            }
            finally
            {
                if (fs != null)

                    fs.Close();
            }
            return inUse;//true表示正在使用,false没有使用
        }
        private void set_EventReadset()
        {
            PrintName = DTIni.ReadValue("Print", "PrintName");
            PaperSize = DTIni.ReadValue("Print", "PaperSize");
            PhotoExe =  DTIni.ReadValue("Other", "PhotoExe");
        }

        private static void Current_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("程序发生错误请重新尝试！");
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("程序遇到未知错误！请将" + Directory.GetCurrentDirectory() + "目录下error文件中的错误日志发给管理员");
            var er = new Errorlog(ErrorPath);
            er.WriteError(e.ExceptionObject.ToString());

        }

        private void but_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PrintFile != null && !ReferenceEquals(but_cut.Content, "确认"))
                {

                    if (PrintName != "" && PaperSize != "" &&
                        DTTOOLS.Print.PrintTools.GetLocalPrinters().Contains(PrintName))
                    {
                        var p = new DTPrint(PrintName, PaperSize, false, new System.Drawing.Printing.Margins(0, 0, 0, 0));

                        var b = (Bitmap) GDITools.LoadBitmap(PrintFile).Clone();

                        if (p.DocumentPrint.DefaultPageSettings.PaperSize.Width >
                            p.DocumentPrint.DefaultPageSettings.PaperSize.Height)
                        {
                            b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }
                        else if (b.Width > b.Height)
                        {
                            b.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }

                        if (Vprint.IsChecked != null && (bool) Vprint.IsChecked)
                        {
                            if (Cut_Print.IsChecked != null && (bool) Cut_Print.IsChecked)
                            {
                                p.PrintView(b, true);
                            }
                            else
                            {
                                p.PrintView(b, false);
                            }
                            b.Dispose();
                        }
                        else
                        {

                            if (Cut_Print.IsChecked != null && (bool)Cut_Print.IsChecked)
                            {
                                p.Print(b, true);
                            }
                            else
                            {
                                p.Print(b, false);
                            }
                            b.Dispose();
                        }
                    }
                    else
                    {
                        MessageBox.Show("无法检测到您配置的打印机，将启用默认打印机预览");
                        var p = new DTPrint();
                        //p.PrintView((Bitmap)System.Drawing.Image.FromFile(PrintFile), true);
                        p.Print((Bitmap) System.Drawing.Image.FromFile(PrintFile), true);
                    }
                }
                else
                {
                    MessageBox.Show("请先选择打印的照片或确认剪裁");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("未知错误！请查看错误日志信息");
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void but_Set_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var set = new UserSet();
                set.EventReadset += set_EventReadset;
                set.Show();
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void but_min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void but_closed_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void list_userlist_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ListUserlist.SelectedIndex == -1) return;
            if (ListUserlist.ItemsSource == null || ListUserlist.SelectedItem == null) return;
            var filldata = new Thread(Filling) {IsBackground = true};
            filldata.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(PrintFile)) return;

            if (ImageShowImage.Visibility == Visibility.Visible)
            {
                but_cut.Content = "确认";
                if (!File.Exists(PrintFile)) return;
                byte[] bytes = File.ReadAllBytes(PrintFile);
                MemoryStream m = new MemoryStream(bytes);
                m.Seek(0, SeekOrigin.Begin);
                var bit = new BitmapImage();
                bit.BeginInit();
                bit.StreamSource = m;
                if (Angle != 0) bit.Rotation = Angle == 90 ? Rotation.Rotate90 : Rotation.Rotate270;

                bit.EndInit();
               
                ImageShowImage.Source = bit;
                GDITools.FlushMemory();
                cut.Visibility = Visibility.Visible;
                cut.LoadCutImage((BitmapImage)ImageShowImage.Source.Clone());
                ImageShowImage.Visibility = Visibility.Hidden;
            }
            else
            {              
                ImageShowImage.Source = cut.GetCutImage();
                cut.Visibility = Visibility.Hidden;
                ImageShowImage.Visibility = Visibility.Visible;
                var wb = (WriteableBitmap) ImageShowImage.Source;
                var tempprint = Directory.GetCurrentDirectory() + "\\" + "temp.jpg";
                var tempBitmap = wb.GetBitmap();
                var pb = new Bitmap(tempBitmap.Width, tempBitmap.Height);
                var g = Graphics.FromImage(pb);
                g.DrawImage(tempBitmap, 0, 0, tempBitmap.Width, tempBitmap.Height);
                g.Dispose();
                pb.Save(tempprint);
                pb.Dispose();            
                tempBitmap.Dispose();
                PrintFile = tempprint;
                but_cut.Content = "剪裁";
                Angle = 0;
                _priMemoryStream = null;
               
            }

        }

        private void Vprint_Checked(object sender, RoutedEventArgs e)
        {
         
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Angle = 270;
                if (_priMemoryStream == null)
                {
                    _priMemoryStream = new MemoryStream(File.ReadAllBytes(PrintFile));
                    
                }
                _priMemoryStream.Seek(0, SeekOrigin.Begin);    
                var bit = new BitmapImage();
                bit.BeginInit();
                bit.StreamSource = _priMemoryStream;
                bit.Rotation = Rotation.Rotate270;
                bit.EndInit();
                ImageShowImage.Source = bit;
                GDITools.FlushMemory();
            }
            catch (Exception ex)
            {

                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Angle = 90;
                if (_priMemoryStream == null)
                {
                    _priMemoryStream = new MemoryStream(File.ReadAllBytes(PrintFile));

                }
                _priMemoryStream.Seek(0, SeekOrigin.Begin);    
                var bit = new BitmapImage();
                bit.BeginInit();
                bit.StreamSource = _priMemoryStream;
                bit.Rotation = Rotation.Rotate90;
                bit.EndInit();
                ImageShowImage.Source = bit;
                GDITools.FlushMemory();
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }


        }


        public class DataList
        {
            public DateTime Datatime { get; set; }

            public string Imageurl { set; get; }

            public string Text { set; get; }

            public string Id { set; get; }

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Angle = 0;
                if (_priMemoryStream == null)
                {
                    _priMemoryStream = new MemoryStream(File.ReadAllBytes(PrintFile));                 
                }
                _priMemoryStream.Seek(0, SeekOrigin.Begin);
                var bit = new BitmapImage();
                bit.BeginInit();
                bit.StreamSource = _priMemoryStream;
                bit.EndInit();
                ImageShowImage.Source = bit;
                GDITools.FlushMemory();
            }
            catch (Exception ex)
            {
                var er = new Errorlog(ErrorPath);
                er.WriteError(ex);
            }

        } 
        private void but_loadphoto_Click(object sender, RoutedEventArgs e)
        {
            if (but_cut.Content.ToString().Contains("剪裁"))
            {
                var openfile = new OpenFileDialog { Filter = "JPG & PNG(*.jpg & *.png)|*.j*g;*.png" };
                openfile.ShowDialog();

                if (!String.IsNullOrWhiteSpace(openfile.FileName))
                {
                    var fbytes = File.ReadAllBytes(openfile.FileName);             
                    _priMemoryStream = new MemoryStream(File.ReadAllBytes(openfile.FileName));
                    _priMemoryStream.Seek(0, SeekOrigin.Begin);
                    var bit = new BitmapImage();
                    bit.BeginInit();
                    bit.StreamSource = _priMemoryStream;
                    bit.EndInit();
                    ImageShowImage.Source = bit;
                    PrintFile = openfile.FileName;
                    GDITools.FlushMemory();
                }
            }
            else
            {
                MessageBox.Show("请先确认剪裁");
            }

        }

        private void ImageShowImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (!String.IsNullOrWhiteSpace(PhotoExe))
                {
                    ProcessStartInfo info = new ProcessStartInfo(PhotoExe, PrintFile);
                    var dos = new Process();
                    dos.StartInfo = info;
                    dos.Start();
                }
                else
                {
                    Process.Start(PrintFile);
                }
               
            }
        }

    }
}
