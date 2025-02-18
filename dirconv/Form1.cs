using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dirconv
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

         if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
         {
            textBox1.Text = folderBrowserDialog1.SelectedPath;
         }
          
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog2.SelectedPath;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && Directory.Exists(files[0]))
            {
                textBox1.Text = files[0];
            }
            else
            {
                MessageBox.Show("単一フォルダのみをドラッグアンドドロップしてください。(複数のフォルダを指定したい場合はその親フォルダを選択してください。) / Please drag and drop only a single folder. (If you want to specify multiple folders, please select their parent folders.)", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && Directory.Exists(files[0]))
            {
                textBox2.Text = files[0];
            }
            else
            {
                MessageBox.Show("単一フォルダのみをドラッグアンドドロップしてください。(複数のフォルダを指定したい場合はその親フォルダを選択してください。) / Please drag and drop only a single folder. (If you want to specify multiple folders, please select their parent folders.)", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void cpyfs2dest(string srcfs,string src,string dest)
        {
            string soutaipath = getsoutaipath(src, srcfs);
            string newdir=Path.Combine(dest, Path.GetDirectoryName(soutaipath));
            if (!Directory.Exists(newdir))
            {
                Directory.CreateDirectory(newdir); //新規作成
            }
            string destfs = Path.Combine(newdir, Path.GetFileName(srcfs));//[Copy] destination
            File.Copy(srcfs, destfs, true); //上書きtrueでcpy!
        }

        private string getsoutaipath(string basepath,string fullpath)
        {
            Uri baseuri = new Uri(basepath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basepath:basepath+Path.DirectorySeparatorChar);
            Uri fulluri = new Uri(fullpath);
            Uri soutaiuri= baseuri.MakeRelativeUri(fulluri); //relativePath(相対パス)を作って格納
            string soutaipath = Uri.UnescapeDataString(soutaiuri.ToString()); //エスケープさせずにstrに。
            return soutaipath.Replace('/', Path.DirectorySeparatorChar);//ちゃんとした表現に直して結果を引き渡す。
        }

        private void cr8bat(string fs,string opfs)
        {
            string batpath = Path.Combine(Path.GetTempPath(), "cnv.bat"); //一時ファイルとして作成
            string content = $"ffmpeg -y -vn -i \"{fs}\" -vn \"{opfs}\""; //ffmpegで変換(アルバムアートつけてやる方法は煩雑なので端折りました ｽﾏﾅｲ...)
            File.WriteAllText(batpath, content, Encoding.Default); //batファイル作成(UTF-8)
            var processinfo = new ProcessStartInfo(batpath)
            {
                CreateNoWindow = false,
                UseShellExecute = true
            };
            Process.Start(processinfo).WaitForExit(); //変換終了まで待機
            File.Delete(batpath); //一時ファイル削除
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                string src = textBox1.Text;
                string dest = textBox2.Text;
                if (Directory.Exists(src) && Directory.Exists(dest))
                {
                    string[] audiofs = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories); //全ファイル収集
                    foreach (var fs in audiofs)
                    {
                        string ext = Path.GetExtension(fs).ToLower();//小文字化して一致させる
                        if (ext == ".mp3")
                        {
                            cpyfs2dest(fs, src, dest);
                            label12.Text = $"コピーしました:{fs}\nCopied:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                        if (ext == ".flac" || ext == ".m4a" || ext == ".wav" || ext == ".opus" || ext==".ac3" ||ext==".mp4")
                        {
                            string soutaipath = getsoutaipath(src, fs);
                            string newdir = Path.Combine(dest, Path.GetDirectoryName(soutaipath));
                            if (!Directory.Exists(newdir))
                            {
                                Directory.CreateDirectory(newdir); //新規作成
                            }
                            string opfs = Path.Combine(newdir, Path.GetFileNameWithoutExtension(fs) + ".mp3"); //出力ファイル名
                            cr8bat(fs, opfs);
                            label12.Text = $"変換しました:{fs}\nConverted:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                    }
                    label12.Text = "処理が完了しました。\nProcess completed.";
                }
                else { label12.Text = "フォルダが存在しません。\nFolder does not exist."; }
            }
            else if(radioButton2.Checked)
            {
                string src = textBox1.Text;
                string dest = textBox2.Text;
                if (Directory.Exists(src) && Directory.Exists(dest))
                {
                    string[] audiofs = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories); //全ファイル収集
                    foreach (var fs in audiofs)
                    {
                        string ext = Path.GetExtension(fs).ToLower();//小文字化して一致させる
                        if (ext == ".m4a")
                        {
                            cpyfs2dest(fs, src, dest);
                            label12.Text = $"コピーしました:{fs}\nCopied:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                        if (ext == ".flac" || ext == ".mp3" || ext == ".wav" || ext == ".opus" || ext == ".ac3" || ext == ".mp4")
                        {
                            string soutaipath = getsoutaipath(src, fs);
                            string newdir = Path.Combine(dest, Path.GetDirectoryName(soutaipath));
                            if (!Directory.Exists(newdir))
                            {
                                Directory.CreateDirectory(newdir); //新規作成
                            }
                            string opfs = Path.Combine(newdir, Path.GetFileNameWithoutExtension(fs) + ".m4a"); //出力ファイル名
                            cr8bat(fs, opfs);
                            label12.Text = $"変換しました:{fs}\nConverted:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                    }
                    label12.Text = "処理が完了しました。\nProcess completed.";
                }
                else { label12.Text = "フォルダが存在しません。\nFolder does not exist."; }
            }
            else if(radioButton3.Checked)
            {
                string src = textBox1.Text;
                string dest = textBox2.Text;
                if (Directory.Exists(src) && Directory.Exists(dest))
                {
                    string[] audiofs = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories); //全ファイル収集
                    foreach (var fs in audiofs)
                    {
                        string ext = Path.GetExtension(fs).ToLower();//小文字化して一致させる
                        if (ext == ".flac")
                        {
                            cpyfs2dest(fs, src, dest);
                            label12.Text = $"コピーしました:{fs}\nCopied:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                        if (ext == ".mp3" || ext == ".m4a" || ext == ".wav" || ext == ".opus" || ext == ".ac3" || ext == ".mp4")
                        {
                            string soutaipath = getsoutaipath(src, fs);
                            string newdir = Path.Combine(dest, Path.GetDirectoryName(soutaipath));
                            if (!Directory.Exists(newdir))
                            {
                                Directory.CreateDirectory(newdir); //新規作成
                            }
                            string opfs = Path.Combine(newdir, Path.GetFileNameWithoutExtension(fs) + ".flac"); //出力ファイル名
                            cr8bat(fs, opfs);
                            label12.Text = $"変換しました:{fs}\nConverted:{fs}";
                            Application.DoEvents();//GUI更新作業
                        }
                    }
                    label12.Text = "処理が完了しました。\nProcess completed.";
                }
                else { label12.Text = "フォルダが存在しません。\nFolder does not exist."; }
            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Form2を表示
            Form2 f2 = new Form2();
            f2.Show();
        }
    }
}
