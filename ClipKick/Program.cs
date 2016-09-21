using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipKick
{
    class ClipKick
    {
        static string target = string.Empty;

        static void Main(string[] args)
        {
            // クリップボードチェック
            var t = new System.Threading.Thread(ClipKick.GetClipboardText);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();

            // コマンドライン引数の配列取得
            var cmds = Environment.GetCommandLineArgs();

            // 引数およびターゲット文字列なしの場合は何もしない
            if (cmds.Count() < 2 || string.IsNullOrEmpty(target)) {
                return;
            }

            // 実行プログラム名確定
            var strProgram = cmds[1];
            if (ClipKick.CheckFile(strProgram)) {
                var ext = Path.GetExtension(strProgram).ToLower();
                if (ext.Equals(".exe") == false && ext.Equals(".bat") == false) {
                    return;
                }
            } else {
                return;
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
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = strProgram;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            if (bForceDir) {
                p.StartInfo.Arguments = string.Format("\"{0}\"", directory);
            }
            else {
                p.StartInfo.Arguments = string.Format("\"{0}\"", arguments.ToString());
            }
            p.Start();
        }

        static void GetClipboardText()
        {
            if (Clipboard.ContainsText()) {
                target = Clipboard.GetText();
            }
        }

        static bool CheckFile(string path)
        {
            return File.Exists(path);
        }

        static bool CheckAndModifyDirectory(ref string path)
        {
            if (Directory.Exists(path)) {
                if (path.EndsWith("\\") == false) {
                    path += "\\";
                }
                return true;
            }
            return false;
        }
    }
}
