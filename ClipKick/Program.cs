using System;
using System.IO;
using System.Linq;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace ClipKick
{
    class ClipKick
    {
        static string target = string.Empty;
        static bool bIsOSX = false;

        static void Main(string[] args)
        {
            var xpc_service_name = System.Environment.GetEnvironmentVariable("XPC_SERVICE_NAME");
            if (string.IsNullOrEmpty(xpc_service_name) == false) {
                if (xpc_service_name.Contains("apple") == true) {
                    bIsOSX = true;
                }
            }

#if WINDOWS
            // クリップボードチェック
             var t = new System.Threading.Thread(ClipKick.GetClipboardText);
             t.SetApartmentState(System.Threading.ApartmentState.STA);
             t.Start();
             t.Join();
#endif
            if (bIsOSX) {
                var procClip = new System.Diagnostics.Process();
                procClip.StartInfo.FileName = "pbpaste";
                procClip.StartInfo.CreateNoWindow = true;
                procClip.StartInfo.UseShellExecute = false;
                procClip.StartInfo.RedirectStandardOutput = true;
                procClip.StartInfo.RedirectStandardInput = false;
                procClip.Start();
                target = procClip.StandardOutput.ReadToEnd();
            }

            // コマンドライン引数の配列取得
            var cmds = Environment.GetCommandLineArgs();

            // 引数およびターゲット文字列なしの場合は何もしない
            if (cmds.Count() < 2 || string.IsNullOrEmpty(target)) {
                return;
            }

            // 実行プログラム名確定
            var strProgram = cmds[1];
            if  (ClipKick.CheckFile(strProgram)) {
                var ext = Path.GetExtension(strProgram).ToLower();
                if (ext.Equals(".exe") == false && ext.Equals(".bat") == false) {
                    return;
                }
            } else {
                if (bIsOSX == false) {
                    return;
                }
            }

            // 引数の強制ディクレクトリ指定フラグ
            var bForceDir = false;
            if (cmds.Count() > 1) {
                if (cmds[2].ToLower().Equals("-d")) {
                    bForceDir = true;
                }
            }

            // ターゲットディレクトリの準備
            int skip = (bForceDir) ? 3 : 2;
            var arguments = cmds.Skip(skip).Take(cmds.Count() - skip).ToArray();
            var directory = target;
            if (string.IsNullOrEmpty(directory)) {
                if (arguments.Count() == 0) {
                    return;
                }
                directory = arguments[0];
            }

            // 強制ディクレクトリ指定時の処理
            if (bForceDir) {
                if (ClipKick.CheckFile(directory)) {
                    System.IO.DirectoryInfo hDirInfo = System.IO.Directory.GetParent(directory);
                    directory = hDirInfo.FullName;
                }
                if (ClipKick.CheckAndModifyDirectory(ref directory) == false) {
                    return;
                }
            }

            // 指定プログラムをスタートする
            var procKick = new System.Diagnostics.Process();
            procKick.StartInfo.FileName = strProgram;
            procKick.StartInfo.CreateNoWindow = true;
            procKick.StartInfo.UseShellExecute = false;
            if (bForceDir) {
                procKick.StartInfo.Arguments = string.Format("\"{0}\"", directory);
            }
            else {
                procKick.StartInfo.Arguments = string.Format("\"{0}\"", arguments.ToString());
            }
            procKick.Start();
        }

#if WINDOWS
         static void GetClipboardText()
         {
             if (Clipboard.ContainsText()) {
                 target = Clipboard.GetText();
             }
         }
#endif

        static bool CheckFile(string path)
        {
            return File.Exists(path);
        }

        static bool CheckAndModifyDirectory(ref string path)
        {
            if (Directory.Exists(path)) {
                var separator = Path.DirectorySeparatorChar.ToString();
                if (path.EndsWith(separator) == false) {
                    path += separator;
                }
                return true;
            }
            return false;
        }
    }
}
