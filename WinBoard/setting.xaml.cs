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


using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace WinBoard
{
    /// <summary>
    /// setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : Window
    {
        string Roaming, winboard, config;
        public Setting()
        {
            InitializeComponent();
            Roaming = ApplicationUtility.GetUserAppDataPath();
            winboard = Roaming + @"\WinBoard";
            config = winboard + @"\config.json";
            AppPath appPath = new AppPath();
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
                //appPath.ApplicationPath = pathText.Text;
                //appPath.BackgroundPath = backgroundPath.Text;
                //var json = JsonConvert.SerializeObject(appPath);
                //using (var sw = new StreamWriter(@"./Resources/config.json", false, System.Text.Encoding.UTF8))
                //{
                //    sw.Write(json);
                //}
            }
            else
            {
                using (var sr = new StreamReader(System.IO.Path.GetFullPath(config), System.Text.Encoding.UTF8))
                {
                    var json = sr.ReadToEnd();
                    appPath = JsonConvert.DeserializeObject<AppPath>(json);
                }

                if (appPath.ApplicationPath != null && Directory.Exists(appPath.ApplicationPath)) pathText.Text = appPath.ApplicationPath;
                if (appPath.BackgroundPath != null && File.Exists(appPath.BackgroundPath)) backgroundPath.Text = appPath.BackgroundPath;
                r_SearchColor.Text = appPath.r_SearchColor.ToString();
                g_SearchColor.Text = appPath.g_SearchColor.ToString();
                b_SearchColor.Text = appPath.b_SearchColor.ToString();
            }
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            UpdateParam();
        }

        void UpdateParam()
        {
            var newPath = pathText.Text;
            var bgPath = backgroundPath.Text;

            AppPath appPath = new AppPath();
            appPath.ApplicationPath = newPath;
            if (File.Exists(bgPath) && (bgPath.EndsWith(".png") || bgPath.EndsWith(".jpg") || bgPath.EndsWith(".jpeg")))
            {
                appPath.BackgroundPath = bgPath;
            }
            else
            {
                appPath.BackgroundPath = "";
            }
            appPath.r_SearchColor = int.Parse(r_SearchColor.Text);
            appPath.g_SearchColor = int.Parse(g_SearchColor.Text);
            appPath.b_SearchColor = int.Parse(b_SearchColor.Text);
            var json = JsonConvert.SerializeObject(appPath);
            using (var sw = new StreamWriter(config, false, System.Text.Encoding.UTF8))
            {
                sw.Write(json);
            }

            DashboardParam.ApplicaitonPath = appPath.ApplicationPath;
            DashboardParam.BackgroundPath = appPath.BackgroundPath;
            DashboardParam.rSerachColor = appPath.r_SearchColor;
            DashboardParam.gSearchColor = appPath.g_SearchColor;
            DashboardParam.bSearchColor = appPath.b_SearchColor;
            var observers = Application.Current.Windows.OfType<IObserver>();
            foreach (var observer in observers)
                observer.Update();
        }
    }

    [JsonObject("AppPath")]
    class AppPath
    {
        [JsonProperty("ApplicationPath")]
        public string ApplicationPath { get; set; }

        [JsonProperty("BackgroundPath")]
        public string BackgroundPath { get; set; }

        [JsonProperty("r_SearchColor")]
        public int r_SearchColor { get; set; }

        [JsonProperty("g_SearchColor")]
        public int g_SearchColor { get; set; }

        [JsonProperty("b_SearchColor")]
        public int b_SearchColor { get; set; }
    }

}
