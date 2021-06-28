using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Shell;

namespace WinBoard
{
    public class AppShortcut
    {
        private string FilePath { get; }
        public string name { get; }
        public readonly Button button;
        private System.Windows.Controls.Image image;

        #region Lable Property

        private System.Windows.Media.Color LabelColor = Colors.Gray;
        private float LabelSize = 14;

        #endregion

        int face = 130;

        public AppShortcut(string filePath)
        {
            //アプリケーションの情報の取得
            this.FilePath = filePath; //path
            var info = new FileInfo(filePath);
            name = info.Name; //FileName

            this.button = GenerateButton();
        }

        public Button GenerateButton()
        {
            Button button = new Button
            {
                Content = this.name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                Background = new SolidColorBrush(Colors.Transparent),
                Width = face,
                Height = face
            };
            //StackPanelを配下に追加
            StackPanel panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };

            //Image
            image = new System.Windows.Controls.Image();

            using (var f = ShellFile.FromFilePath(FilePath))
            {
                f.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                image.Source = f.Thumbnail.BitmapSource;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
            }

            button.Content = panel;
            panel.Children.Add(image); //buttonに画像の登録

            //Label
            Label label = new Label
            {
                Foreground = new SolidColorBrush(LabelColor),
                FontSize = LabelSize,
                Content = Path.GetFileNameWithoutExtension(this.name),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(label); //buttonにラベルの登録

            //市の設定

            //クリックイベントを登録
            button.Click += AppExecute;

            ContextMenu menu = new ContextMenu();
            MenuItem ItemToDelete = new MenuItem();
            ItemToDelete.Click += (sender, e) => { this.ShortcutDelete(sender, e); };
            ItemToDelete.Header = "Delete";
            menu.Items.Add(ItemToDelete);

            button.ContextMenu = menu;

            return button;
        }

        //ClickEvent
        public void AppExecute(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.FilePath);
            }
            catch
            {
                MessageBox.Show("ファイルが開けませんでした");
            }

            //アプリの終了
            //System.Windows.Application.Current.Shutdown();
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }


        public void ShortcutDelete(object sender, EventArgs e)
        {
            File.Delete(FilePath);
            DeletedEvent?.Invoke();
        }

        public event Action DeletedEvent;

        private static void BitmapResize(Bitmap bitmap)
        {
            float hRes = bitmap.HorizontalResolution;
            float vRes = bitmap.VerticalResolution;
            //Console.WriteLine("hres: " + hRes);
            if (hRes != 96.0F || vRes != 96.0F)
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
        bool GetOriginalPath(string path, out string OriginalPath)
        {
            OriginalPath = null;
            string extension = Path.GetExtension(path);
            if (extension == ".lnk")
            {
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut
                    shortcut = shell.CreateShortcut(path) as IWshRuntimeLibrary.IWshShortcut;
                OriginalPath = shortcut.TargetPath.ToString();
                return true;
            }

            return false;
        }
    }
}