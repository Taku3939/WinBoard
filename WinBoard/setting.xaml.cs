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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace WinBoard
{
    /// <summary>
    /// setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Setting : Window
    {
        string ResourcesPath = @"./Resources";
        string config = @"./Resources/config.json";
        public Setting()
        {
            InitializeComponent();
            AppPath appPath = new AppPath();
            //Resourcesフォルダがなかったら作る
            if (!Directory.Exists(ResourcesPath))
            {
                MessageBox.Show("Resourcesフォルダが見つかりませんでした\n" + "ファルダを自動作成します");
                Directory.CreateDirectory(@"./Resources");
            }

            //Configがなかったら作る
            if (!File.Exists(config))
            {
                MessageBox.Show("configファイルが見つかりませんでした\n" + "ファイルを自動作成します");
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
            appPath.BackgroundPath = bgPath;
            appPath.r_SearchColor = int.Parse(r_SearchColor.Text);
            appPath.g_SearchColor = int.Parse(g_SearchColor.Text);
            appPath.b_SearchColor = int.Parse(b_SearchColor.Text);
            var json = JsonConvert.SerializeObject(appPath);
            using (var sw = new StreamWriter(@"./Resources/config.json", false, System.Text.Encoding.UTF8))
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
