/*!
 * Process.js v13
 *
 * Copyright (c) 2020 Takuya Isaki
 *
 * Released under the MIT license.
 * see https://opensource.org/licenses/MIT
 *
 * The inherits function is:
 * ISC license | https://github.com/isaacs/inherits/blob/master/LICENSE
 */

using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media;
using System.Security.AccessControl;
using System.Security.Principal;
using Newtonsoft.Json;

namespace WinBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObserver
    {

        private List<AppShortcut> appIcons = new List<AppShortcut> { };
       // private string config = @"./Resources/config.json";

        public MainWindow()
        {
            string config = ApplicationUtility.GetUserAppDataPath() + @"\WinBoard\config.json";
            //string ResourcesPath = @"./Resources";
            InitializeComponent();
            if (!File.Exists(config))
            {
                return;
            }
            AppPath appPath = new AppPath();
           
            using (var sr = new StreamReader(System.IO.Path.GetFullPath(config), System.Text.Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                appPath = JsonConvert.DeserializeObject<AppPath>(json);
            }

            if (appPath.r_SearchColor < 256 && appPath.r_SearchColor > 0)
                DashboardParam.rSerachColor = appPath.r_SearchColor;
            else
                DashboardParam.rSerachColor = 0;

            if (appPath.g_SearchColor < 256 && appPath.g_SearchColor > 0)
                DashboardParam.gSearchColor = appPath.g_SearchColor;
            else
                DashboardParam.gSearchColor = 0;
            if (appPath.b_SearchColor < 256 && appPath.b_SearchColor > 0)
                DashboardParam.bSearchColor = appPath.b_SearchColor;
            else
                DashboardParam.bSearchColor = 0;

            TextColorChange(SearchText, (byte)DashboardParam.rSerachColor, (byte)DashboardParam.gSearchColor, (byte)DashboardParam.bSearchColor);

            //背景画像が指定されていれば読み込む
            if (appPath.BackgroundPath != null && File.Exists(appPath.BackgroundPath))
            {
                DashboardParam.BackgroundPath = appPath.BackgroundPath;
                ChangeBg(Path.GetFullPath(DashboardParam.BackgroundPath));
            }

            if (appPath.ApplicationPath != null && Directory.Exists(appPath.ApplicationPath))
            {
                DashboardParam.ApplicaitonPath = appPath.ApplicationPath;
                Reload();
            }


        }

        void AllReload()
        {
            if (DashboardParam.rSerachColor > 255 && DashboardParam.rSerachColor < 0)
                DashboardParam.rSerachColor = 0;
            if (DashboardParam.gSearchColor > 255 && DashboardParam.gSearchColor < 0)
                DashboardParam.gSearchColor = 0;
            if (DashboardParam.bSearchColor > 255 && DashboardParam.bSearchColor < 0)
                DashboardParam.bSearchColor = 0;
            TextColorChange(SearchText,  Convert.ToByte(DashboardParam.rSerachColor), Convert.ToByte(DashboardParam.gSearchColor), Convert.ToByte(DashboardParam.bSearchColor));
            if(File.Exists(DashboardParam.BackgroundPath))
            { 
                ChangeBg(Path.GetFullPath(DashboardParam.BackgroundPath));
            }
            Reload();
        }

        /// <summary>
        /// TextBoxのForegroundの色変更
        /// </summary>
        /// <param name="box"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        void TextColorChange(TextBox box, byte r, byte g, byte b)
        {
            System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(r, g, b);
            System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(color);
            box.Foreground = brush;
        }

        void ChangeBg(string bgPath)
        {
            if ( (bgPath.EndsWith(".png") || bgPath.EndsWith(".jpg") || bgPath.EndsWith(".jpeg"))) {
                BitmapImage bitmap = new BitmapImage(new Uri(bgPath));
                ImageBrush bg = new ImageBrush(bitmap);
                bg.Opacity = 0.2;
                var windows = Application.Current.Windows.OfType<MainWindow>();
                //var main = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsActive);
                foreach (var window in windows)
                    window.Background = bg;
            }
            else
            {
                DashboardParam.BackgroundPath = "";
            }
        }

        private void AppConstruct(List<AppShortcut> applications)
        {
            //if (appIcons.Count != 0)
            //{
            //    foreach (var app in appIcons)
            //    {
            //        WrapPanel.Children.Remove(app.button);
            //    }
            //}

            foreach (var appIcon in applications)
            {
                Thickness thickness = new Thickness();
                thickness.Left = 15;
                thickness.Right = 15;
                thickness.Top = 15;
                thickness.Bottom = 15;
                appIcon.button.Margin = thickness;
                WrapPanel.Children.Add(appIcon.button);
            }
        }


        #region MainMethod
        private void GetMatrix(int n)
        {
            int col, row;
            int index = n / 40;
            col = n % 8 + 2;
            row = (n / 8 + 1) % 5;
        }
        static void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Windows.Application.Current.Shutdown();
        //}

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (appIcons.Count != 0)
            {
                foreach (var app in appIcons)
                {
                    WrapPanel.Children.Remove(app.button);
                }
            }

            var shortcuts = appIcons.Where(x => x.name.ToUpper().Contains(SearchText.Text.ToUpper())).ToList();

            foreach (var appIcon in shortcuts)
            {
                Thickness thickness = new Thickness();
                thickness.Left = 15;
                thickness.Right = 15;
                thickness.Top = 15;
                thickness.Bottom = 15;
                appIcon.button.Margin = thickness;
                WrapPanel.Children.Add(appIcon.button);
            }
        }


        #endregion

        private void MainPanel_Drop(object sender, DragEventArgs e)
        {
           

            var files = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList();
            if (files.Count == 0) return;
            foreach (var file in files)
            {
                if (file.EndsWith(".lnk"))
                {
                    var filename = Path.GetFileName(file);
                    var dest = Path.Combine(DashboardParam.ApplicaitonPath, filename);
                    File.Copy(file, dest, true);
                    SetFullContorol(dest);
                }
                else
                {
                    var f = ApplicationUtility.CreateShrotcutFile(file, DashboardParam.ApplicaitonPath);
                    SetFullContorol(f);
                }

            }
            Reload();

        }
        void Reload()
        {
            if (!Directory.Exists(DashboardParam.ApplicaitonPath)) return;
            
            if (appIcons.Count != 0)
            {
                foreach (var app in appIcons)
                {   
                    WrapPanel.Children.Remove(app.button);
                }
            }
            appIcons.Clear();

            var files = Directory.GetFiles(DashboardParam.ApplicaitonPath).Where(f => f.EndsWith(".lnk")).ToList();
            foreach (var file in files)
            {
                AppShortcut appIcon = new AppShortcut(file);
                appIcon.DeletedEvent += Reload;
                appIcons.Add(appIcon);
            }

            foreach (var appIcon in appIcons)
            {
                Thickness thickness = new Thickness();
                thickness.Left = 15;
                thickness.Right = 15;
                thickness.Top = 15;
                thickness.Bottom = 15;
                appIcon.button.Margin = thickness;
                WrapPanel.Children.Add(appIcon.button);
            }
        }

        /// <summary>
        /// Windowを最小化する
        /// </summary>
        private void WindowMinimize()
        {
            this.WindowState = WindowState.Minimized;
        }

      

        static void SetFullContorol(string path)
        {
            FileSystemAccessRule rule = new FileSystemAccessRule(new NTAccount("SYSTEM"),
              FileSystemRights.FullControl,
              InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
              PropagationFlags.None,
              AccessControlType.Allow);
            DirectorySecurity security = Directory.GetAccessControl(path);
            security.SetAccessRule(rule);
            Directory.SetAccessControl(path, security);
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            Setting window = new Setting();
            window.Show();
        }

        void IObserver.Update()
        {
            AllReload();
        }
    }


    public class AppShortcut
    {
        public Bitmap bmp { get; set; }
        public string path { get; set; }
        public string name { get; set; }

        //private bool LabelVisibliy = true;
        public Button button;
        private System.Windows.Controls.Image image;

        #region Lable Property
        private System.Windows.Media.Color LableColor = Colors.Gray;
        private float LableSize = 14;
        #endregion
        
        int face = 130;
        public AppShortcut(string path)
        {
            //アプリケーションの情報の取得
            this.path = path;                       //path
            var info = new FileInfo(path);
            name = info.Name;                       //FileName
            SetButton(out this.button);         //Buttonの作成       
        }

        public void SetButton(out Button button)
        {
            button = new Button();
            button.Content = this.name;
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.VerticalAlignment = VerticalAlignment.Center;
            button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            button.Background = new SolidColorBrush(Colors.Transparent);
            this.button.Width = face;
            this.button.Height = face;

            //StackPanelを配下に追加
            StackPanel panel = new StackPanel();
            // panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Center;


            ////EXEファイルのpath取得
            //string originalPath;
            //if (GetOriginalpath(path, out originalPath))
            //{
            //    path = originalPath;
            //}

            //Image
            image = new System.Windows.Controls.Image();
            //IntPtr handle = IntPtr.Zero;
            //IntPtr handle = System.Drawing.SystemIcons.Application.Handle;
           // BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, null);

            using (var f = ShellFile.FromFilePath(path))
            {
                f.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                // Console.WriteLine("x,y = " + f.Thumbnail.BitmapSource.DpiX + f.Thumbnail.BitmapSource.DpiY);
                //BitmapResize(f.Thumbnail.Bitmap);
                image.Source = f.Thumbnail.BitmapSource;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
            }        

            this.button.Content = panel;
            panel.Children.Add(image);            //buttonに画像の登録

            //Label
            Label label = new Label();
            label.Foreground = new SolidColorBrush( LableColor);
            label.FontSize = LableSize;
            label.Content = Path.GetFileNameWithoutExtension(this.name);
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            panel.Children.Add(label);            //buttonにラベルの登録
           
            //市の設定

            //クリックイベントを登録
            this.button.Click += (sender, e) => { this.AppExecute(sender, e); };
            // this.button.MouseRightButtonDown += (sender, e) => { this.}

            ContextMenu menu = new ContextMenu();
            MenuItem ItemToDelete = new MenuItem();
            ItemToDelete.Click += (sender, e) => { this.ShortcutDelete(sender, e); };
            ItemToDelete.Header = "Delete";
            //MenuItem ItemToRename = new MenuItem();
            //ItemToRename.Click += (sender, e) => { this.RenameFile(sender, e); };
            
            //MenuItem ItemToCreate = new MenuItem();
            //ItemToCreate.Click += (sender, e) => { this.}
            menu.Items.Add(ItemToDelete);

            this.button.ContextMenu = menu;
        }

        //ClickEvent
        public void AppExecute(object sender, RoutedEventArgs e)
        {

            try
            {
                System.Diagnostics.Process.Start(this.path);
            }
            catch
            {

                MessageBox.Show("ファイルが開けませんでした");
            }
            //アプリの終了
            //System.Windows.Application.Current.Shutdown();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;

        }



        public void ShortcutDelete(object sender, EventArgs e)
        {
            File.Delete(path);
            DeletedEvent?.Invoke();
            
        }

        public event Action DeletedEvent;
        //void Destroy()
        //{
        //    MainWindow.ReloadEvent();
        //}

        private static void BitmapResize(Bitmap bitmap)
        {
            float hRes = bitmap.HorizontalResolution;
            float vRes = bitmap.VerticalResolution;
            //Console.WriteLine("hres: " + hRes);
            if(hRes != 96.0F || vRes != 96.0F)
            {
                 //Bitmap map = new Bitmap(bitmap.Width, bitmap.Height);
                bitmap.SetResolution(96.0F, 96.0F);
            }

            
        }

        /// <summary>
        /// ショートカットの元のファイルパスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <param name="OriginalPath"></param>
        /// <returns></returns>
        bool GetOriginalpath(string path, out string OriginalPath)
        {
            OriginalPath = null;
            string extension = Path.GetExtension(path);
            if(extension == ".lnk")
            {
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(path) as IWshRuntimeLibrary.IWshShortcut;
                OriginalPath = shortcut.TargetPath.ToString();
                return true;
            }

            return false;
        }
        
    }

    public static class DashboardParam
    {
        public static string ApplicaitonPath { get; set; }
        public static string BackgroundPath { get; set; }
        public static int rSerachColor { get; set; }
        public static int gSearchColor { get; set; }
        public static int bSearchColor { get; set; }
    }

    interface IObserver
    {
        void Update();
    }

    public static class ApplicationUtility
    {
        /// <summary>
        /// ショートカットファイルを作成します.
        /// </summary>
        /// <param name="filePath">ファイルのパス</param>
        /// <param name="destDir">出力先のディレクトリのパス</param>
        public static string CreateShrotcutFile(string filePath, string destDir)
        {

            Console.WriteLine(new FileInfo(filePath).Name + " のショートカットを作成します。");

            // ショートカットファイル名
            string shortcutFile = Path.GetFileNameWithoutExtension(filePath) + @".lnk";

            // 作成するショートカットのパス
            string shortcutPath = destDir + @"\" + shortcutFile;

            // リフレクションでWSHオブジェクトを作成
            // GUIDは WSH のCLSID
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));

            // WSHでショートカットを作成
            var shortcut = shell.CreateShortcut(shortcutPath);

            // ショートカットのリンク先設定
            shortcut.TargetPath = filePath;

            // アイコンのパスを設定
            // ショートカットの元となるファイルから 0番目 のアイコンを指定
            shortcut.IconLocation = filePath + ",0";

            // ショートカットを保存
            shortcut.Save();

            //FileSystemAccessRule rule = new FileSystemAccessRule(new NTAccount("SYSTEM"),
            //    FileSystemRights.FullControl,
            //    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
            //    PropagationFlags.None,
            //    AccessControlType.Allow);
            //DirectorySecurity security = Directory.GetAccessControl(destDir);
            //security.SetAccessRule(rule);
            //Directory.SetAccessControl(destDir, security);
            // Shell、COMオブジェクトの解放
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            return shortcutPath;

        }


        /// <summary>
        /// AppData\Roamingディレクトリの取得
        /// </summary>
        /// <returns></returns>
        public static string GetUserAppDataPath()
        {
            string path = string.Empty;
            Assembly assembly;
            Type at;
            object[] r;

            // Get the .EXE assembly
            assembly = Assembly.GetEntryAssembly();
            // Get a 'Type' of the AssemblyCompanyAttribute
            at = typeof(AssemblyCompanyAttribute);
            // Get a collection of custom attributes from the .EXE assembly
            r = assembly.GetCustomAttributes(at, false);
            // Get the Company Attribute
            AssemblyCompanyAttribute ct =
                          ((AssemblyCompanyAttribute)(r[0]));
            // Build the User App Data Path
            path = Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData);
            //path += @"\" + ct.Company;
            //path += @"\" + assembly.GetName().Version.ToString();
            return path;
        }
    }
}