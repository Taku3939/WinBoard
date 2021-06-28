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


using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace WinBoard
{
    /// <summary>
    /// setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting
    {
        string Roaming, winboard, config;

        public Setting()
        {
            InitializeComponent();
            Roaming = ApplicationUtility.GetUserAppDataPath();
            winboard = Roaming + @"\WinBoard";
            config = winboard + @"\config.json";


            //WinBoardフォルダがなかったら作る
            if (!Directory.Exists(winboard))
            {
                MessageBox.Show("WinBoardフォルダが見つかりませんでした\n" + "ファルダを自動作成します");
                Directory.CreateDirectory(winboard);
            }

            //Configがなかったら作る
            if (!File.Exists(config))
            {
                MessageBox.Show("config.jsonファイルが見つかりませんでした\n" + "ファイルを自動作成します");
                var fs = File.Create(config);
                fs.Close();
            }
            else
            {
                AppPath appPath;
                using (var sr = new StreamReader(Path.GetFullPath(config), System.Text.Encoding.UTF8))
                {
                    var json = sr.ReadToEnd();
                    try
                    {
                        appPath = JsonConvert.DeserializeObject<AppPath>(json);
                    }
                    catch
                    {
                        //エラー時は文面を作成しなおす
                        appPath = new AppPath() {ApplicationPath = "", BackgroundPath = "", color = new Color(0, 0, 0)};
                        var str = JsonConvert.SerializeObject(appPath);
                        using (var sw = new StreamWriter(config, false, System.Text.Encoding.UTF8)) sw.Write(str);
                    }
                }

                if (appPath.ApplicationPath != null && Directory.Exists(appPath.ApplicationPath))
                    pathText.Text = appPath.ApplicationPath;
                if (appPath.BackgroundPath != null && File.Exists(appPath.BackgroundPath))
                    backgroundPath.Text = appPath.BackgroundPath;
                r_SearchColor.Text = appPath.color.r.ToString("D", new NumberFormatInfo());
                g_SearchColor.Text = appPath.color.g.ToString("D", new NumberFormatInfo());
                b_SearchColor.Text = appPath.color.b.ToString("D", new NumberFormatInfo());
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) => UpdateParam();

        void UpdateParam()
        {
            var newPath = pathText.Text;
            var bgPath = backgroundPath.Text;

            AppPath appPath = new AppPath();
            appPath.ApplicationPath = newPath;
            appPath.BackgroundPath = File.Exists(bgPath) && (bgPath.EndsWith(".png", StringComparison.Ordinal) ||
                                                             bgPath.EndsWith(".jpg", StringComparison.Ordinal) ||
                                                             bgPath.EndsWith(".jpeg", StringComparison.Ordinal))
                ? bgPath
                : "";

            var r = int.Parse(r_SearchColor.Text, NumberStyles.Integer, new NumberFormatInfo());
            var g = int.Parse(g_SearchColor.Text, NumberStyles.Integer, new NumberFormatInfo());
            var b = int.Parse(b_SearchColor.Text, NumberStyles.Integer, new NumberFormatInfo());
            appPath.color = new Color(r, g, b);
            var json = JsonConvert.SerializeObject(appPath);
            using (var sw = new StreamWriter(config, false, System.Text.Encoding.UTF8)) sw.Write(json);

            var observers = Application.Current.Windows.OfType<MainWindow>();
            foreach (var observer in observers)
                observer.AllReload(appPath.color, appPath.BackgroundPath, appPath.ApplicationPath);
        }
    }
}