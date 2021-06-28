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
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;
using static WinBoard.ApplicationUtility;

namespace WinBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<AppShortcut> _appIcons = new List<AppShortcut>();
        private static readonly string Config = GetUserAppDataPath() + @"\WinBoard\config.json";

        public MainWindow()
        {
            InitializeComponent();

            // configファイルがなかった場合終了
            if (!File.Exists(Config))
                return;

            //Jsonのロード
            using (var sr = new StreamReader(Path.GetFullPath(Config), System.Text.Encoding.UTF8))
            {
                var json = sr.ReadToEnd();

                var appPath = JsonConvert.DeserializeObject<AppPath>(json);

                Console.WriteLine(appPath.color.r + appPath.color.g + appPath.color.b);
                AllReload(appPath.color, appPath.BackgroundPath, appPath.ApplicationPath);
            }
        }

        /// <summary>
        /// すべてのコンテンツのロード
        /// </summary>
        /// <param name="color"></param>
        /// <param name="bgPath"></param>
        /// <param name="applicationPath"></param>
        public void AllReload(Color color, string bgPath, string applicationPath)
        {
            LoadColor(SearchText, color);
            //背景画像が指定されていれば読み込む
            if (!string.IsNullOrEmpty(bgPath) && File.Exists(bgPath)) ChangeBg(Path.GetFullPath(bgPath));


            //ショートカットをロードする
            if (!string.IsNullOrEmpty(applicationPath) && Directory.Exists(applicationPath)) _ = Reload(applicationPath);
        }

        /// <summary>
        /// TextBoxのForegroundの色変更
        /// </summary>
        /// <param name="box"></param>
        /// <param name="color"></param>
        void LoadColor(TextBox box, Color color)
        {
            var r = (byte) (int) Clamp(color.r, 0f, 255f);
            var g = (byte) (int) Clamp(color.g, 0f, 255f);
            var b = (byte) (int) Clamp(color.b, 0f, 255f);
            System.Windows.Media.Color c = System.Windows.Media.Color.FromRgb(r, g, b);
            System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(c);
            box.Foreground = brush;
        }


        void ChangeBg(string bgPath)
        {
            if (bgPath.EndsWith(".png", StringComparison.Ordinal) ||
                bgPath.EndsWith(".jpg", StringComparison.Ordinal) ||
                bgPath.EndsWith(".jpeg", StringComparison.Ordinal))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(bgPath));
                ImageBrush bg = new ImageBrush(bitmap) {Opacity = 0.2};
                var windows = Application.Current.Windows.OfType<MainWindow>();
                foreach (var window in windows)
                    window.Background = bg;
            }
        }

        #region MainMethod

        static void Exit() => System.Windows.Application.Current.Shutdown();


        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appIcons.Count != 0)
            {
                foreach (var app in _appIcons)
                {
                    WrapPanel.Children.Remove(app.button);
                }
            }

            var shortcuts = _appIcons.Where(x =>
                x.name.ToUpper(CultureInfo.CurrentCulture)
                    .Contains(SearchText.Text.ToUpper(CultureInfo.CurrentCulture))).ToList();

            foreach (var appIcon in shortcuts)
            {
                Thickness thickness = new Thickness {Left = 15, Right = 15, Top = 15, Bottom = 15};
                appIcon.button.Margin = thickness;
                WrapPanel.Children.Add(appIcon.button);
            }
        }

        #endregion


        /// <summary>
        /// ファイルをドラッグアンドドロップしたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPanel_Drop(object sender, DragEventArgs e)
        {
            var files = ((DataObject) e.Data).GetFileDropList().Cast<string>().ToList();
            if (files.Count == 0) return;
            AppPath appPath;
            //Jsonのロード

            using (var sr = new StreamReader(Path.GetFullPath(Config), System.Text.Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                appPath = JsonConvert.DeserializeObject<AppPath>(json);
            }

            foreach (var file in files)
            {
                string dest;
                if (file.EndsWith(".lnk", StringComparison.Ordinal))
                {
                    var filename = Path.GetFileName(file);
                    dest = Path.Combine(appPath.ApplicationPath, filename);
                    File.Copy(file, dest, true);
                }
                else
                    dest = ApplicationUtility.CreateShortcutFile(file, appPath.ApplicationPath);

                SetFullControl(dest);
            }

            _ = Reload(appPath.ApplicationPath);
        }

        private async Task Reload(string path)
        {
            //ショートカットの保存先ディレクトリがない場合失敗
            if (!Directory.Exists(path))
            {
                MessageBox.Show($"{path} にディレクトリが存在しません");
                return;
            }

            if (_appIcons.Count != 0)
                foreach (var app in _appIcons)
                    WrapPanel.Children.Remove(app.button);

            _appIcons.Clear();

            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                //ファイルの非同期取得
                await Task.Run(() =>
                {
                    var files = Directory.GetFiles(path)
                        .Where(f => f.EndsWith(".lnk", StringComparison.Ordinal)).ToList();


                    foreach (var filepath in files)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        //UIスレッドに戻す
                        this.Dispatcher.Invoke(() =>
                        {
                            AppShortcut appIcon = new AppShortcut(filepath);
                            appIcon.DeletedEvent += () => _ = Reload(path);
                            Thickness thickness = new Thickness {Left = 15, Right = 15, Top = 15, Bottom = 15};
                            appIcon.button.Margin = thickness;
                            WrapPanel.Children.Add(appIcon.button);
                            _appIcons.Add(appIcon);

                        }, System.Windows.Threading.DispatcherPriority.Background, cts.Token);
                    }

                }, cts.Token);
            }
            catch
            {
                // ignored
            }
            finally
            {
                cts.Dispose();
            }
        }

        /// <summary>
        /// Windowを最小化する
        /// </summary>
        private void WindowMinimize() => this.WindowState = WindowState.Minimized;

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            Setting window = new Setting();
            window.Show();
        }
    }
}