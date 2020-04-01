using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media;
using System.Collections;
using System.Windows.Media.Animation;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections.ObjectModel;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WinBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    { 
        
        private List<AppShortcut> appIcons = new List<AppShortcut> { };
        
       
        public MainWindow()
        { 

            InitializeComponent();
            
            Reload();
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

        private void button_ColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color> e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (appIcons.Count != 0)
            {
                foreach (var app in appIcons)
                {
                    WrapPanel.Children.Remove(app.button);
                }
            }

            var shortcuts = appIcons.Where(x => x.name.ToUpper().Contains(SerchText.Text.ToUpper())).ToList();

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
                    var dest = Path.Combine(DashboardParam.PATH, filename);
                    File.Copy(file, dest, true);
                    SetFullContorol(dest);
                }
                else
                {
                    var f = CreateShrotcutFile(file, DashboardParam.PATH);
                    SetFullContorol(f);
                }

            }
            Reload();

        }
        void Reload()
        {
            if (appIcons.Count != 0)
            {
                foreach (var app in appIcons)
                {   
                    WrapPanel.Children.Remove(app.button);
                }
            }
            appIcons.Clear();

            var files = Directory.GetFiles(DashboardParam.PATH).Where(f => f.EndsWith(".lnk")).ToList();
            foreach (var file in files)
            {
                AppShortcut appIcon = new AppShortcut(file);
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
    }


    public class AppShortcut
    {
        public Bitmap bmp { get; set; }
        public string path { get; set; }
        public string name { get; set; }

        private static bool LabelVisibliy = true;
        public Button button;
        private System.Windows.Controls.Image image;

        #region Lable Property
        private System.Windows.Media.Color LableColor = Colors.Gray;
        private float LableSize = 14;
        #endregion

        
        int face = 110;
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
            IntPtr handle = System.Drawing.SystemIcons.Application.Handle;
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, null);

            using (var f = ShellFile.FromFilePath(path))
            {
                f.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
               // Console.WriteLine("x,y = " + f.Thumbnail.BitmapSource.DpiX + f.Thumbnail.BitmapSource.DpiY);
                BitmapResize(f.Thumbnail.Bitmap);
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
            label.Content = this.name;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            panel.Children.Add(label);            //buttonにラベルの登録
           
  

            //市の設定

            //クリックイベントを登録
            this.button.Click += (sender, e) => { this.AppExecute(sender, e); };
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
            System.Windows.Application.Current.Shutdown();
        }

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
        public static string PATH  = @"D:\Application";
        public static void update(string test)
        {
            PATH = test;
        }
    }
}