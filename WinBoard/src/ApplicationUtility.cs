using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WinBoard
{
     public static class ApplicationUtility
    {
        /// <summary>
        /// ショートカットファイルを作成します.
        /// </summary>
        /// <param name="filePath">ファイルのパス</param>
        /// <param name="destDir">出力先のディレクトリのパス</param>
        public static string CreateShortcutFile(string filePath, string destDir)
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
        
        
        public static void SetFullControl(string path)
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
        
        
        /// <summary>
        /// 値を最小値以上最大値以下の値にして返す
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max)
        {
            float v = value;
            if (value < min) v = min;
            else if (value > max) v = max;

            return v;
        }
        
    }
}